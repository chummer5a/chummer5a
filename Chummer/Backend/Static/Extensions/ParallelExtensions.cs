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
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    public static class ParallelExtensions
    {
        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.ForEach while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="lstItems">Enumerable supplying the source of items for the code we want to run in parallel.</param>
        /// <param name="funcCodeToRun">Code to run in parallel.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachAsync<TSource>(IEnumerable<TSource> lstItems, Func<TSource, Task> funcCodeToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Acquire enumerator first so that if we have a collection with a read lock, we acquire it before we create our buffer
            IEnumerator<TSource> objEnumerator = lstItems.GetEnumerator();
            try
            {
                int intBufferSize = Utils.MaxParallelBatchSize;
                if (lstItems is IReadOnlyCollection<TSource> lstItemsCollection)
                    intBufferSize = Math.Min(intBufferSize, lstItemsCollection.Count);
                using (new FetchSafelyFromSafeObjectPool<List<Task>>(Utils.TaskListPool, out List<Task> lstBuffer))
                {
                    token.ThrowIfCancellationRequested();
                    int i = 0;
                    while (objEnumerator.MoveNext())
                    {
                        token.ThrowIfCancellationRequested();
                        lstBuffer.Add(funcCodeToRun(objEnumerator.Current));
                        if (++i == intBufferSize)
                        {
                            await Task.WhenAll(lstBuffer).ConfigureAwait(false);
                            lstBuffer.Clear();
                            i = 0;
                        }
                    }
                    if (i > 0)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstBuffer).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else if (objEnumerator is IDisposable objDisposable)
                    objDisposable.Dispose();
            }
        }

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.ForEach while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="lstItems">Enumerable supplying the source of items for the code we want to run in parallel.</param>
        /// <param name="funcCodeToRun">Code to run in parallel.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachAsync(IEnumerable lstItems, Func<object, Task> funcCodeToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Acquire enumerator first so that if we have a collection with a read lock, we acquire it before we create our buffer
            IEnumerator objEnumerator = lstItems.GetEnumerator();
            try
            {
                int intBufferSize = Utils.MaxParallelBatchSize;
                if (lstItems is ICollection lstItemsCollection)
                    intBufferSize = Math.Min(intBufferSize, lstItemsCollection.Count);
                using (new FetchSafelyFromSafeObjectPool<List<Task>>(Utils.TaskListPool, out List<Task> lstBuffer))
                {
                    token.ThrowIfCancellationRequested();
                    int i = 0;
                    while (objEnumerator.MoveNext())
                    {
                        token.ThrowIfCancellationRequested();
                        lstBuffer.Add(funcCodeToRun(objEnumerator.Current));
                        if (++i == intBufferSize)
                        {
                            await Task.WhenAll(lstBuffer).ConfigureAwait(false);
                            lstBuffer.Clear();
                            i = 0;
                        }
                    }
                    if (i > 0)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstBuffer).ConfigureAwait(false);
                    }

                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else if (objEnumerator is IDisposable objDisposable)
                    objDisposable.Dispose();
            }
        }

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.ForEach while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="lstItems">Enumerable supplying the source of items for the code we want to run in parallel.</param>
        /// <param name="funcCodeToRun">Code to run in parallel.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachAsync<TSource>(IAsyncEnumerable<TSource> lstItems, Func<TSource, Task> funcCodeToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Acquire enumerator first so that if we have a collection with a read lock, we acquire it before we create our buffer
            IEnumerator<TSource> objEnumerator = await lstItems.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intBufferSize = Utils.MaxParallelBatchSize;
                if (lstItems is IAsyncReadOnlyCollection<TSource> lstItemsCollection)
                    intBufferSize = Math.Min(intBufferSize, await lstItemsCollection.GetCountAsync(token).ConfigureAwait(false));
                using (new FetchSafelyFromSafeObjectPool<List<Task>>(Utils.TaskListPool, out List<Task> lstBuffer))
                {
                    token.ThrowIfCancellationRequested();

                    int i = 0;
                    while (objEnumerator.MoveNext())
                    {
                        token.ThrowIfCancellationRequested();
                        lstBuffer.Add(funcCodeToRun(objEnumerator.Current));
                        if (++i == intBufferSize)
                        {
                            await Task.WhenAll(lstBuffer).ConfigureAwait(false);
                            lstBuffer.Clear();
                            i = 0;
                        }
                    }

                    // Keep this last part inside the bloc before enumerator is disposed because we want to maintain the read lock on collections that have one until the parallel methods have completed
                    if (i > 0)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstBuffer).ConfigureAwait(false);
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

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.ForEach while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="lstItems">Enumerable supplying the source of items for the code we want to run in parallel.</param>
        /// <param name="funcCodeToRun">Code to run in parallel.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachAsync<TSource>(IAsyncEnumerable<TSource> lstItems, Action<TSource> funcCodeToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Acquire enumerator first so that if we have a collection with a read lock, we acquire it before we create our buffer
            IEnumerator<TSource> objEnumerator = await lstItems.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intBufferSize = Utils.MaxParallelBatchSize;
                if (lstItems is IAsyncReadOnlyCollection<TSource> lstItemsCollection)
                    intBufferSize = Math.Min(intBufferSize, await lstItemsCollection.GetCountAsync(token).ConfigureAwait(false));
                using (new FetchSafelyFromSafeObjectPool<List<Task>>(Utils.TaskListPool, out List<Task> lstBuffer))
                {
                    token.ThrowIfCancellationRequested();
                    int i = 0;
                    while (objEnumerator.MoveNext())
                    {
                        token.ThrowIfCancellationRequested();
                        lstBuffer.Add(Task.Run(() => funcCodeToRun(objEnumerator.Current), token));
                        if (++i == intBufferSize)
                        {
                            await Task.WhenAll(lstBuffer).ConfigureAwait(false);
                            lstBuffer.Clear();
                            i = 0;
                        }
                    }

                    // Keep this last part inside the bloc before enumerator is disposed because we want to maintain the read lock on collections that have one until the parallel methods have completed
                    if (i > 0)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstBuffer).ConfigureAwait(false);
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

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.ForEach while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="lstItems">Enumerable supplying the source of items for the code we want to run in parallel.</param>
        /// <param name="funcCodeToRun">Code to run in parallel.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>List of the results of <paramref name="funcCodeToRun"/> when run over <paramref name="lstItems"/>.</returns>
        public static async Task<List<TResult>> ForEachAsync<TSource, TResult>(IEnumerable<TSource> lstItems, Func<TSource, Task<TResult>> funcCodeToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Acquire enumerator first so that if we have a collection with a read lock, we acquire it before we create our buffer
            IEnumerator<TSource> objEnumerator = lstItems.GetEnumerator();
            try
            {
                token.ThrowIfCancellationRequested();
                List<TResult> lstReturn;
                int intBufferSize = Utils.MaxParallelBatchSize;
                if (lstItems is IReadOnlyCollection<TSource> lstItemsCollection)
                {
                    lstReturn = new List<TResult>(lstItemsCollection.Count);
                    intBufferSize = Math.Min(lstItemsCollection.Count, intBufferSize);
                }
                else
                {
                    lstReturn = new List<TResult>(intBufferSize);
                }
                List<Task<TResult>> lstBuffer = new List<Task<TResult>>(intBufferSize);
                token.ThrowIfCancellationRequested();
                int i = 0;
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    lstBuffer.Add(funcCodeToRun(objEnumerator.Current));
                    if (++i == intBufferSize)
                    {
                        lstReturn.AddRange(await Task.WhenAll(lstBuffer).ConfigureAwait(false));
                        lstBuffer.Clear();
                        i = 0;
                    }
                }
                if (i > 0)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.WhenAll(lstBuffer).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    foreach (Task<TResult> tskLoop in lstBuffer)
                        lstReturn.Add(await tskLoop.ConfigureAwait(false));
                }
                return lstReturn;
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else if (objEnumerator is IDisposable objDisposable)
                    objDisposable.Dispose();
            }
        }

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.ForEach while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="lstItems">Enumerable supplying the source of items for the code we want to run in parallel.</param>
        /// <param name="funcCodeToRun">Code to run in parallel.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>List of the results of <paramref name="funcCodeToRun"/> when run over <paramref name="lstItems"/>.</returns>
        public static async Task<List<TResult>> ForEachAsync<TResult>(IEnumerable lstItems, Func<object, Task<TResult>> funcCodeToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Acquire enumerator first so that if we have a collection with a read lock, we acquire it before we create our buffer
            IEnumerator objEnumerator = lstItems.GetEnumerator();
            try
            {
                token.ThrowIfCancellationRequested();
                List<TResult> lstReturn;
                int intBufferSize = Utils.MaxParallelBatchSize;
                if (lstItems is ICollection lstItemsCollection)
                {
                    lstReturn = new List<TResult>(lstItemsCollection.Count);
                    intBufferSize = Math.Min(lstItemsCollection.Count, intBufferSize);
                }
                else
                {
                    lstReturn = new List<TResult>(intBufferSize);
                }
                List<Task<TResult>> lstBuffer = new List<Task<TResult>>(intBufferSize);
                token.ThrowIfCancellationRequested();
                int i = 0;
                token.ThrowIfCancellationRequested();
                while (objEnumerator.MoveNext())
                {
                    lstBuffer.Add(funcCodeToRun(objEnumerator.Current));
                    if (++i == intBufferSize)
                    {
                        lstReturn.AddRange(await Task.WhenAll(lstBuffer).ConfigureAwait(false));
                        lstBuffer.Clear();
                        i = 0;
                    }
                }

                if (i > 0)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.WhenAll(lstBuffer).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    foreach (Task<TResult> tskLoop in lstBuffer)
                        lstReturn.Add(await tskLoop.ConfigureAwait(false));
                }
                return lstReturn;
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else if (objEnumerator is IDisposable objDisposable)
                    objDisposable.Dispose();
            }
        }

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.ForEach while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="lstItems">Enumerable supplying the source of items for the code we want to run in parallel.</param>
        /// <param name="funcCodeToRun">Code to run in parallel.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>List of the results of <paramref name="funcCodeToRun"/> when run over <paramref name="lstItems"/>.</returns>
        public static async Task<List<TResult>> ForEachAsync<TSource, TResult>(IAsyncEnumerable<TSource> lstItems, Func<TSource, Task<TResult>> funcCodeToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Acquire enumerator first so that if we have a collection with a read lock, we acquire it before we create our buffer
            IEnumerator<TSource> objEnumerator = await lstItems.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                List<TResult> lstReturn;
                int intBufferSize = Utils.MaxParallelBatchSize;
                if (lstItems is IAsyncReadOnlyCollection<TSource> lstItemsCollection)
                {
                    lstReturn = new List<TResult>(await lstItemsCollection.GetCountAsync(token).ConfigureAwait(false));
                    intBufferSize = Math.Min(await lstItemsCollection.GetCountAsync(token).ConfigureAwait(false), intBufferSize);
                }
                else
                {
                    lstReturn = new List<TResult>(intBufferSize);
                }
                List<Task<TResult>> lstBuffer = new List<Task<TResult>>(intBufferSize);
                token.ThrowIfCancellationRequested();
                int i = 0;
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    lstBuffer.Add(funcCodeToRun(objEnumerator.Current));
                    if (++i == intBufferSize)
                    {
                        lstReturn.AddRange(await Task.WhenAll(lstBuffer).ConfigureAwait(false));
                        lstBuffer.Clear();
                        i = 0;
                    }
                }

                // Keep this last part inside the bloc before enumerator is disposed because we want to maintain the read lock on collections that have one until the parallel methods have completed
                if (i > 0)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.WhenAll(lstBuffer).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    foreach (Task<TResult> tskLoop in lstBuffer)
                        lstReturn.Add(await tskLoop.ConfigureAwait(false));
                }
                return lstReturn;
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
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.ForEach while respecting the max parallel batch size we have set in Utils.
        /// </summary>
        /// <param name="lstItems">Enumerable supplying the source of items for the code we want to run in parallel.</param>
        /// <param name="funcCodeToRun">Code to run in parallel.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>List of the results of <paramref name="funcCodeToRun"/> when run over <paramref name="lstItems"/>.</returns>
        public static async Task<List<TResult>> ForEachAsync<TSource, TResult>(IAsyncEnumerable<TSource> lstItems, Func<TSource, TResult> funcCodeToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Acquire enumerator first so that if we have a collection with a read lock, we acquire it before we create our buffer
            IEnumerator<TSource> objEnumerator = await lstItems.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                List<TResult> lstReturn;
                int intBufferSize = Utils.MaxParallelBatchSize;
                if (lstItems is IAsyncReadOnlyCollection<TSource> lstItemsCollection)
                {
                    lstReturn = new List<TResult>(await lstItemsCollection.GetCountAsync(token).ConfigureAwait(false));
                    intBufferSize = Math.Min(await lstItemsCollection.GetCountAsync(token).ConfigureAwait(false), intBufferSize);
                }
                else
                {
                    lstReturn = new List<TResult>(intBufferSize);
                }
                List<Task<TResult>> lstBuffer = new List<Task<TResult>>(intBufferSize);
                token.ThrowIfCancellationRequested();
                int i = 0;
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    lstBuffer.Add(Task.Run(() => funcCodeToRun(objEnumerator.Current), token));
                    if (++i == intBufferSize)
                    {
                        lstReturn.AddRange(await Task.WhenAll(lstBuffer).ConfigureAwait(false));
                        lstBuffer.Clear();
                        i = 0;
                    }
                }

                // Keep this last part inside the bloc before enumerator is disposed because we want to maintain the read lock on collections that have one until the parallel methods have completed
                if (i > 0)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.WhenAll(lstBuffer).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    foreach (Task<TResult> tskLoop in lstBuffer)
                        lstReturn.Add(await tskLoop.ConfigureAwait(false));
                }
                return lstReturn;
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
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.ForEach while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="lstItems">Enumerable supplying the source of items for the code we want to run in parallel.</param>
        /// <param name="funcCodeToRunWithPotentialBreak">Code to run in parallel. CancellationTokenSource argument is for early termination of the loop, request it to cancel (but don't throw an exception) to make the loop terminate early.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachAsync<TSource>(IEnumerable<TSource> lstItems, Func<TSource, CancellationTokenSource, Task> funcCodeToRunWithPotentialBreak, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Acquire enumerator first so that if we have a collection with a read lock, we acquire it before we create our buffer
            IEnumerator<TSource> objEnumerator = lstItems.GetEnumerator();
            try
            {
                token.ThrowIfCancellationRequested();
                using (CancellationTokenSource objBreakLoop = new CancellationTokenSource())
                {
                    token.ThrowIfCancellationRequested();
                    CancellationToken objBreakToken = objBreakLoop.Token;
                    using (CancellationTokenTaskSource objBreakTokenTaskSource = new CancellationTokenTaskSource(objBreakToken))
                    {
                        Task objBreakTokenTask = objBreakTokenTaskSource.Task;
                        int intBufferSize = Utils.MaxParallelBatchSize;
                        if (lstItems is IReadOnlyCollection<TSource> lstItemsCollection)
                            intBufferSize = Math.Min(intBufferSize, lstItemsCollection.Count);
                        using (new FetchSafelyFromSafeObjectPool<List<Task>>(Utils.TaskListPool, out List<Task> lstBuffer))
                        {
                            token.ThrowIfCancellationRequested();
                            int i = 0;
                            while (objEnumerator.MoveNext())
                            {
                                token.ThrowIfCancellationRequested();
                                lstBuffer.Add(funcCodeToRunWithPotentialBreak(objEnumerator.Current, objBreakLoop));
                                if (i == intBufferSize)
                                {
                                    if (await Task.WhenAny(Task.WhenAll(lstBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                        return;
                                    lstBuffer.Clear();
                                    i = 0;
                                }
                            }

                            if (i > 0)
                            {
                                token.ThrowIfCancellationRequested();
                                if (await Task.WhenAny(Task.WhenAll(lstBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                    return;
                            }
                        }
                    }
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else if (objEnumerator is IDisposable objDisposable)
                    objDisposable.Dispose();
            }
        }

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.ForEach while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="lstItems">Enumerable supplying the source of items for the code we want to run in parallel.</param>
        /// <param name="funcCodeToRunWithPotentialBreak">Code to run in parallel. CancellationTokenSource argument is for early termination of the loop, request it to cancel (but don't throw an exception) to make the loop terminate early.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachAsync(IEnumerable lstItems, Func<object, CancellationTokenSource, Task> funcCodeToRunWithPotentialBreak, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Acquire enumerator first so that if we have a collection with a read lock, we acquire it before we create our buffer
            IEnumerator objEnumerator = lstItems.GetEnumerator();
            try
            {
                token.ThrowIfCancellationRequested();
                using (CancellationTokenSource objBreakLoop = new CancellationTokenSource())
                {
                    token.ThrowIfCancellationRequested();
                    CancellationToken objBreakToken = objBreakLoop.Token;
                    using (CancellationTokenTaskSource objBreakTokenTaskSource = new CancellationTokenTaskSource(objBreakToken))
                    {
                        Task objBreakTokenTask = objBreakTokenTaskSource.Task;
                        int intBufferSize = Utils.MaxParallelBatchSize;
                        if (lstItems is ICollection lstItemsCollection)
                            intBufferSize = Math.Min(lstItemsCollection.Count, intBufferSize);
                        using (new FetchSafelyFromSafeObjectPool<List<Task>>(Utils.TaskListPool, out List<Task> lstBuffer))
                        {
                            token.ThrowIfCancellationRequested();
                            int i = 0;
                            while (objEnumerator.MoveNext())
                            {
                                token.ThrowIfCancellationRequested();
                                lstBuffer.Add(funcCodeToRunWithPotentialBreak(objEnumerator.Current, objBreakLoop));
                                if (++i == intBufferSize)
                                {
                                    if (await Task.WhenAny(Task.WhenAll(lstBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                        return;
                                    lstBuffer.Clear();
                                    i = 0;
                                }
                            }

                            if (i > 0)
                            {
                                token.ThrowIfCancellationRequested();
                                await Task.WhenAny(Task.WhenAll(lstBuffer), objBreakTokenTask).ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else if (objEnumerator is IDisposable objDisposable)
                    objDisposable.Dispose();
            }
        }

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.ForEach while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="lstItems">Enumerable supplying the source of items for the code we want to run in parallel.</param>
        /// <param name="funcCodeToRunWithPotentialBreak">Code to run in parallel. CancellationTokenSource argument is for early termination of the loop, request it to cancel (but don't throw an exception) to make the loop terminate early.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachAsync<TSource>(IAsyncEnumerable<TSource> lstItems, Func<TSource, CancellationTokenSource, Task> funcCodeToRunWithPotentialBreak, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Acquire enumerator first so that if we have a collection with a read lock, we acquire it before we create our buffer
            IEnumerator<TSource> objEnumerator = await lstItems.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                using (CancellationTokenSource objBreakLoop = new CancellationTokenSource())
                {
                    token.ThrowIfCancellationRequested();
                    CancellationToken objBreakToken = objBreakLoop.Token;
                    using (CancellationTokenTaskSource objBreakTokenTaskSource = new CancellationTokenTaskSource(objBreakToken))
                    {
                        Task objBreakTokenTask = objBreakTokenTaskSource.Task;
                        int intBufferSize = Utils.MaxParallelBatchSize;
                        if (lstItems is IAsyncReadOnlyCollection<TSource> lstItemsCollection)
                            intBufferSize = Math.Min(await lstItemsCollection.GetCountAsync(token).ConfigureAwait(false), intBufferSize);
                        using (new FetchSafelyFromSafeObjectPool<List<Task>>(Utils.TaskListPool, out List<Task> lstBuffer))
                        {
                            token.ThrowIfCancellationRequested();
                            int i = 0;
                            while (objEnumerator.MoveNext())
                            {
                                token.ThrowIfCancellationRequested();
                                lstBuffer.Add(funcCodeToRunWithPotentialBreak(objEnumerator.Current, objBreakLoop));
                                if (++i == intBufferSize)
                                {
                                    if (await Task.WhenAny(Task.WhenAll(lstBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                        return;
                                    lstBuffer.Clear();
                                    i = 0;
                                }
                            }

                            // Keep this last part inside the bloc before enumerator is disposed because we want to maintain the read lock on collections that have one until the parallel methods have completed
                            if (i > 0)
                            {
                                token.ThrowIfCancellationRequested();
                                await Task.WhenAny(Task.WhenAll(lstBuffer), objBreakTokenTask).ConfigureAwait(false);
                            }
                        }
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

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.ForEach while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="lstItems">Enumerable supplying the source of items for the code we want to run in parallel.</param>
        /// <param name="funcCodeToRunWithPotentialBreak">Code to run in parallel. CancellationTokenSource argument is for early termination of the loop, request it to cancel (but don't throw an exception) to make the loop terminate early.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachAsync<TSource>(IAsyncEnumerable<TSource> lstItems, Action<TSource, CancellationTokenSource> funcCodeToRunWithPotentialBreak, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Acquire enumerator first so that if we have a collection with a read lock, we acquire it before we create our buffer
            IEnumerator<TSource> objEnumerator = await lstItems.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                using (CancellationTokenSource objBreakLoop = new CancellationTokenSource())
                {
                    token.ThrowIfCancellationRequested();
                    CancellationToken objBreakToken = objBreakLoop.Token;
                    using (CancellationTokenTaskSource objBreakTokenTaskSource = new CancellationTokenTaskSource(objBreakToken))
                    {
                        Task objBreakTokenTask = objBreakTokenTaskSource.Task;
                        int intBufferSize = Utils.MaxParallelBatchSize;
                        if (lstItems is IAsyncReadOnlyCollection<TSource> lstItemsCollection)
                            intBufferSize = Math.Min(await lstItemsCollection.GetCountAsync(token).ConfigureAwait(false), intBufferSize);
                        using (new FetchSafelyFromSafeObjectPool<List<Task>>(Utils.TaskListPool, out List<Task> lstBuffer))
                        {
                            token.ThrowIfCancellationRequested();
                            int i = 0;
                            while (objEnumerator.MoveNext())
                            {
                                token.ThrowIfCancellationRequested();
                                lstBuffer.Add(Task.Run(() => funcCodeToRunWithPotentialBreak(objEnumerator.Current, objBreakLoop), objBreakToken));
                                if (++i == intBufferSize)
                                {
                                    if (await Task.WhenAny(Task.WhenAll(lstBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                        return;
                                    lstBuffer.Clear();
                                    i = 0;
                                }
                            }

                            // Keep this last part inside the bloc before enumerator is disposed because we want to maintain the read lock on collections that have one until the parallel methods have completed
                            if (i > 0)
                            {
                                token.ThrowIfCancellationRequested();
                                await Task.WhenAny(Task.WhenAll(lstBuffer), objBreakTokenTask).ConfigureAwait(false);
                            }
                        }
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

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.ForEach while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="lstItems">Enumerable supplying the source of items for the code we want to run in parallel.</param>
        /// <param name="funcCodeToRunWithPotentialBreak">Code to run in parallel. CancellationTokenSource argument is for early termination of the loop, request it to cancel (but don't throw an exception) to make the loop terminate early.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>List of the results of <paramref name="funcCodeToRun"/> when run over <paramref name="lstItems"/>.</returns>
        public static async Task<List<TResult>> ForEachAsync<TSource, TResult>(IEnumerable<TSource> lstItems, Func<TSource, CancellationTokenSource, Task<TResult>> funcCodeToRunWithPotentialBreak, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Acquire enumerator first so that if we have a collection with a read lock, we acquire it before we create our buffer
            IEnumerator<TSource> objEnumerator = lstItems.GetEnumerator();
            try
            {
                token.ThrowIfCancellationRequested();
                using (CancellationTokenSource objBreakLoop = new CancellationTokenSource())
                {
                    token.ThrowIfCancellationRequested();
                    CancellationToken objBreakToken = objBreakLoop.Token;
                    using (CancellationTokenTaskSource objBreakTokenTaskSource = new CancellationTokenTaskSource(objBreakToken))
                    {
                        Task objBreakTokenTask = objBreakTokenTaskSource.Task;
                        List<TResult> lstReturn;
                        int intBufferSize = Utils.MaxParallelBatchSize;
                        if (lstItems is IReadOnlyCollection<TSource> lstItemsCollection)
                        {
                            lstReturn = new List<TResult>(lstItemsCollection.Count);
                            intBufferSize = Math.Min(intBufferSize, lstItemsCollection.Count);
                        }
                        else
                            lstReturn = new List<TResult>(intBufferSize);
                        List<Task<TResult>> lstBuffer = new List<Task<TResult>>(intBufferSize);
                        token.ThrowIfCancellationRequested();
                        int i = 0;
                        while (objEnumerator.MoveNext())
                        {
                            token.ThrowIfCancellationRequested();
                            lstBuffer.Add(funcCodeToRunWithPotentialBreak(objEnumerator.Current, objBreakLoop));
                            if (++i == intBufferSize)
                            {
                                Task<TResult[]> tskEnsemble = Task.WhenAll(lstBuffer);
                                if (await Task.WhenAny(tskEnsemble, objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                {
                                    token.ThrowIfCancellationRequested();
                                    foreach (Task<TResult> tskLoop in lstBuffer)
                                    {
                                        if (!tskLoop.IsCanceled)
                                            lstReturn.Add(await tskLoop.ConfigureAwait(false));
                                    }
                                }
                                else
                                {
                                    token.ThrowIfCancellationRequested();
                                    lstReturn.AddRange(await tskEnsemble.ConfigureAwait(false));
                                }
                                lstBuffer.Clear();
                                i = 0;
                            }
                        }
                        if (i > 0)
                        {
                            token.ThrowIfCancellationRequested();
                            if (await Task.WhenAny(Task.WhenAll(lstBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                            {
                                token.ThrowIfCancellationRequested();
                                foreach (Task<TResult> tskLoop in lstBuffer)
                                {
                                    if (!tskLoop.IsCanceled)
                                        lstReturn.Add(await tskLoop.ConfigureAwait(false));
                                }
                            }
                            else
                            {
                                token.ThrowIfCancellationRequested();
                                foreach (Task<TResult> tskLoop in lstBuffer)
                                    lstReturn.Add(await tskLoop.ConfigureAwait(false));
                            }
                        }
                        return lstReturn;
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

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.ForEach while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="lstItems">Enumerable supplying the source of items for the code we want to run in parallel.</param>
        /// <param name="funcCodeToRunWithPotentialBreak">Code to run in parallel. CancellationTokenSource argument is for early termination of the loop, request it to cancel (but don't throw an exception) to make the loop terminate early.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>List of the results of <paramref name="funcCodeToRun"/> when run over <paramref name="lstItems"/>.</returns>
        public static async Task<List<TResult>> ForEachAsync<TResult>(IEnumerable lstItems, Func<object, CancellationTokenSource, Task<TResult>> funcCodeToRunWithPotentialBreak, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Acquire enumerator first so that if we have a collection with a read lock, we acquire it before we create our buffer
            IEnumerator objEnumerator = lstItems.GetEnumerator();
            try
            {
                token.ThrowIfCancellationRequested();
                using (CancellationTokenSource objBreakLoop = new CancellationTokenSource())
                {
                    token.ThrowIfCancellationRequested();
                    CancellationToken objBreakToken = objBreakLoop.Token;
                    using (CancellationTokenTaskSource objBreakTokenTaskSource = new CancellationTokenTaskSource(objBreakToken))
                    {
                        Task objBreakTokenTask = objBreakTokenTaskSource.Task;
                        List<TResult> lstReturn;
                        int intBufferSize = Utils.MaxParallelBatchSize;
                        if (lstItems is ICollection lstItemsCollection)
                        {
                            lstReturn = new List<TResult>(lstItemsCollection.Count);
                            intBufferSize = Math.Min(intBufferSize, lstItemsCollection.Count);
                        }
                        else
                            lstReturn = new List<TResult>(intBufferSize);
                        List<Task<TResult>> lstBuffer = new List<Task<TResult>>(intBufferSize);
                        token.ThrowIfCancellationRequested();
                        int i = 0;
                        while (objEnumerator.MoveNext())
                        {
                            lstBuffer.Add(funcCodeToRunWithPotentialBreak(objEnumerator.Current, objBreakLoop));
                            if (++i == intBufferSize)
                            {
                                Task<TResult[]> tskEnsemble = Task.WhenAll(lstBuffer);
                                if (await Task.WhenAny(tskEnsemble, objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                {
                                    token.ThrowIfCancellationRequested();
                                    foreach (Task<TResult> tskLoop in lstBuffer)
                                    {
                                        if (!tskLoop.IsCanceled)
                                            lstReturn.Add(await tskLoop.ConfigureAwait(false));
                                    }
                                    return lstReturn;
                                }
                                else
                                {
                                    token.ThrowIfCancellationRequested();
                                    lstReturn.AddRange(await tskEnsemble.ConfigureAwait(false));
                                }
                                lstBuffer.Clear();
                                i = 0;
                            }
                        }

                        if (i > 0)
                        {
                            token.ThrowIfCancellationRequested();
                            if (await Task.WhenAny(Task.WhenAll(lstBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                            {
                                token.ThrowIfCancellationRequested();
                                foreach (Task<TResult> tskLoop in lstBuffer)
                                {
                                    if (!tskLoop.IsCanceled)
                                        lstReturn.Add(await tskLoop.ConfigureAwait(false));
                                }
                            }
                            else
                            {
                                token.ThrowIfCancellationRequested();
                                foreach (Task<TResult> tskLoop in lstBuffer)
                                    lstReturn.Add(await tskLoop.ConfigureAwait(false));
                            }
                        }
                        return lstReturn;
                    }
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else if (objEnumerator is IDisposable objDisposable)
                    objDisposable.Dispose();
            }
        }

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.ForEach while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="lstItems">Enumerable supplying the source of items for the code we want to run in parallel.</param>
        /// <param name="funcCodeToRunWithPotentialBreak">Code to run in parallel. CancellationTokenSource argument is for early termination of the loop, request it to cancel (but don't throw an exception) to make the loop terminate early.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>List of the results of <paramref name="funcCodeToRun"/> when run over <paramref name="lstItems"/>.</returns>
        public static async Task<List<TResult>> ForEachAsync<TSource, TResult>(IAsyncEnumerable<TSource> lstItems, Func<TSource, CancellationTokenSource, Task<TResult>> funcCodeToRunWithPotentialBreak, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Acquire enumerator first so that if we have a collection with a read lock, we acquire it before we create our buffer
            IEnumerator<TSource> objEnumerator = await lstItems.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                using (CancellationTokenSource objBreakLoop = new CancellationTokenSource())
                {
                    token.ThrowIfCancellationRequested();
                    CancellationToken objBreakToken = objBreakLoop.Token;
                    using (CancellationTokenTaskSource objBreakTokenTaskSource = new CancellationTokenTaskSource(objBreakToken))
                    {
                        Task objBreakTokenTask = objBreakTokenTaskSource.Task;
                        List<TResult> lstReturn;
                        int intBufferSize = Utils.MaxParallelBatchSize;
                        if (lstItems is IAsyncReadOnlyCollection<TSource> lstItemsCollection)
                        {
                            intBufferSize = Math.Min(await lstItemsCollection.GetCountAsync(token).ConfigureAwait(false), intBufferSize);
                            lstReturn = new List<TResult>(await lstItemsCollection.GetCountAsync(token).ConfigureAwait(false));
                        }
                        else
                            lstReturn = new List<TResult>(intBufferSize);
                        List<Task<TResult>> lstBuffer = new List<Task<TResult>>(intBufferSize);
                        token.ThrowIfCancellationRequested();
                        int i = 0;
                        while (objEnumerator.MoveNext())
                        {
                            token.ThrowIfCancellationRequested();
                            lstBuffer.Add(funcCodeToRunWithPotentialBreak(objEnumerator.Current, objBreakLoop));
                            if (++i == intBufferSize)
                            {
                                Task<TResult[]> tskEnsemble = Task.WhenAll(lstBuffer);
                                if (await Task.WhenAny(tskEnsemble, objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                {
                                    token.ThrowIfCancellationRequested();
                                    foreach (Task<TResult> tskLoop in lstBuffer)
                                    {
                                        if (!tskLoop.IsCanceled)
                                            lstReturn.Add(await tskLoop.ConfigureAwait(false));
                                    }
                                    return lstReturn;
                                }
                                else
                                {
                                    token.ThrowIfCancellationRequested();
                                    lstReturn.AddRange(await tskEnsemble.ConfigureAwait(false));
                                }
                                lstBuffer.Clear();
                                i = 0;
                            }
                        }

                        // Keep this last part inside the bloc before enumerator is disposed because we want to maintain the read lock on collections that have one until the parallel methods have completed
                        if (i > 0)
                        {
                            token.ThrowIfCancellationRequested();
                            if (await Task.WhenAny(Task.WhenAll(lstBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                            {
                                token.ThrowIfCancellationRequested();
                                foreach (Task<TResult> tskLoop in lstBuffer)
                                {
                                    if (!tskLoop.IsCanceled)
                                        lstReturn.Add(await tskLoop.ConfigureAwait(false));
                                }
                            }
                            else
                            {
                                token.ThrowIfCancellationRequested();
                                foreach (Task<TResult> tskLoop in lstBuffer)
                                    lstReturn.Add(await tskLoop.ConfigureAwait(false));
                            }
                        }
                        return lstReturn;
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

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.ForEach while respecting the max parallel batch size we have set in Utils.
        /// </summary>
        /// <param name="lstItems">Enumerable supplying the source of items for the code we want to run in parallel.</param>
        /// <param name="funcCodeToRunWithPotentialBreak">Code to run in parallel. CancellationTokenSource argument is for early termination of the loop, request it to cancel (but don't throw an exception) to make the loop terminate early.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>List of the results of <paramref name="funcCodeToRun"/> when run over <paramref name="lstItems"/>.</returns>
        public static async Task<List<TResult>> ForEachAsync<TSource, TResult>(IAsyncEnumerable<TSource> lstItems, Func<TSource, CancellationTokenSource, TResult> funcCodeToRunWithPotentialBreak, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Acquire enumerator first so that if we have a collection with a read lock, we acquire it before we create our buffer
            IEnumerator<TSource> objEnumerator = await lstItems.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                using (CancellationTokenSource objBreakLoop = new CancellationTokenSource())
                {
                    CancellationToken objBreakToken = objBreakLoop.Token;
                    using (CancellationTokenTaskSource objBreakTokenTaskSource = new CancellationTokenTaskSource(objBreakToken))
                    {
                        Task objBreakTokenTask = objBreakTokenTaskSource.Task;
                        List<TResult> lstReturn;
                        int intBufferSize = Utils.MaxParallelBatchSize;
                        if (lstItems is IAsyncReadOnlyCollection<TSource> lstItemsCollection)
                        {
                            intBufferSize = Math.Min(await lstItemsCollection.GetCountAsync(token).ConfigureAwait(false), intBufferSize);
                            lstReturn = new List<TResult>(await lstItemsCollection.GetCountAsync(token).ConfigureAwait(false));
                        }
                        else
                            lstReturn = new List<TResult>(intBufferSize);
                        List<Task<TResult>> lstBuffer = new List<Task<TResult>>(intBufferSize);
                        token.ThrowIfCancellationRequested();
                        int i = 0;
                        while (objEnumerator.MoveNext())
                        {
                            token.ThrowIfCancellationRequested();
                            lstBuffer.Add(Task.Run(() => funcCodeToRunWithPotentialBreak(objEnumerator.Current, objBreakLoop), objBreakToken));
                            if (++i == intBufferSize)
                            {
                                Task<TResult[]> tskEnsemble = Task.WhenAll(lstBuffer);
                                if (await Task.WhenAny(tskEnsemble, objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                {
                                    token.ThrowIfCancellationRequested();
                                    foreach (Task<TResult> tskLoop in lstBuffer)
                                    {
                                        if (!tskLoop.IsCanceled)
                                            lstReturn.Add(await tskLoop.ConfigureAwait(false));
                                    }
                                    return lstReturn;
                                }
                                else
                                {
                                    token.ThrowIfCancellationRequested();
                                    lstReturn.AddRange(await tskEnsemble.ConfigureAwait(false));
                                }
                                lstBuffer.Clear();
                                i = 0;
                            }
                        }

                        // Keep this last part inside the bloc before enumerator is disposed because we want to maintain the read lock on collections that have one until the parallel methods have completed
                        if (i > 0)
                        {
                            token.ThrowIfCancellationRequested();
                            if (await Task.WhenAny(Task.WhenAll(lstBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                            {
                                token.ThrowIfCancellationRequested();
                                foreach (Task<TResult> tskLoop in lstBuffer)
                                {
                                    if (!tskLoop.IsCanceled)
                                        lstReturn.Add(await tskLoop.ConfigureAwait(false));
                                }
                            }
                            else
                            {
                                token.ThrowIfCancellationRequested();
                                foreach (Task<TResult> tskLoop in lstBuffer)
                                    lstReturn.Add(await tskLoop.ConfigureAwait(false));
                            }
                        }

                        return lstReturn;
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

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.For while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="intLowerBound">Starting value of the iterating variable (inclusive).</param>
        /// <param name="intUpperBound">Terminating value of the iterating variable (exclusive).</param>
        /// <param name="funcCodeToRun">Code to run in parallel.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Task ForAsync(int intLowerBound, int intUpperBound, Func<int, Task> funcCodeToRun, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            int intLoopLength = intUpperBound - intLowerBound;
            if (intLoopLength <= 0)
                return Task.CompletedTask;
            return Inner();
            async Task Inner()
            {
                int intBufferSize = Math.Min(intLoopLength, Utils.MaxParallelBatchSize);
                using (new FetchSafelyFromSafeObjectPool<List<Task>>(Utils.TaskListPool, out List<Task> lstBuffer))
                {
                    token.ThrowIfCancellationRequested();
                    int i = 0;
                    for (int j = intLowerBound; j < intUpperBound; ++j)
                    {
                        token.ThrowIfCancellationRequested();
                        lstBuffer.Add(funcCodeToRun(j));
                        if (++i == intBufferSize)
                        {
                            await Task.WhenAll(lstBuffer).ConfigureAwait(false);
                            lstBuffer.Clear();
                            i = 0;
                        }
                    }
                    if (i > 0)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstBuffer).ConfigureAwait(false);
                    }

                }
            }
        }

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.For while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="intLowerBound">Starting value of the iterating variable (inclusive).</param>
        /// <param name="intUpperBound">Terminating value of the iterating variable (exclusive).</param>
        /// <param name="funcCodeToRunWithPotentialBreak">Code to run in parallel. CancellationTokenSource argument is for early termination of the loop, request it to cancel (but don't throw an exception) to make the loop terminate early.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Task ForAsync(int intLowerBound, int intUpperBound, Func<int, CancellationTokenSource, Task> funcCodeToRunWithPotentialBreak, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            int intLoopLength = intUpperBound - intLowerBound;
            if (intLoopLength <= 0)
                return Task.CompletedTask;
            return Inner();
            async Task Inner()
            {
                using (CancellationTokenSource objBreakLoop = new CancellationTokenSource())
                {
                    CancellationToken objBreakToken = objBreakLoop.Token;
                    using (CancellationTokenTaskSource objBreakTokenTaskSource = new CancellationTokenTaskSource(objBreakToken))
                    {
                        Task objBreakTokenTask = objBreakTokenTaskSource.Task;
                        int intBufferSize = Math.Min(intLoopLength, Utils.MaxParallelBatchSize);
                        using (new FetchSafelyFromSafeObjectPool<List<Task>>(Utils.TaskListPool, out List<Task> lstBuffer))
                        {
                            token.ThrowIfCancellationRequested();
                            int i = 0;
                            for (int j = intLowerBound; j < intUpperBound; ++j)
                            {
                                token.ThrowIfCancellationRequested();
                                lstBuffer.Add(funcCodeToRunWithPotentialBreak(j, objBreakLoop));
                                if (++i == intBufferSize)
                                {
                                    if (await Task.WhenAny(Task.WhenAll(lstBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                        return;
                                    lstBuffer.Clear();
                                    i = 0;
                                }
                            }
                            if (i > 0)
                            {
                                token.ThrowIfCancellationRequested();
                                await Task.WhenAny(Task.WhenAll(lstBuffer), objBreakTokenTask).ConfigureAwait(false);
                            }

                        }
                    }
                }
            }
        }

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.For while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="intLowerBound">Starting value of the iterating variable (inclusive).</param>
        /// <param name="intUpperBound">Terminating value of the iterating variable (exclusive).</param>
        /// <param name="funcCodeToRun">Code to run in parallel.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>Array of the results of <paramref name="funcCodeToRun"/> when run from <paramref name="intLowerBound"/> (inclusive) to <paramref name="intUpperBound"/> (exclusive).</returns>
        public static Task<TResult[]> ForAsync<TResult>(int intLowerBound, int intUpperBound, Func<int, Task<TResult>> funcCodeToRun, CancellationToken token = default)
        {
            return ForAsync(intLowerBound, intUpperBound, funcCodeToRun, false, token);
        }

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.For while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="intLowerBound">Starting value of the iterating variable (inclusive).</param>
        /// <param name="intUpperBound">Terminating value of the iterating variable (exclusive).</param>
        /// <param name="funcCodeToRun">Code to run in parallel.</param>
        /// <param name="blnPooledArray">Whether the returned array should be one taken from ArrayPool.Shared</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>Array of the results of <paramref name="funcCodeToRun"/> when run from <paramref name="intLowerBound"/> (inclusive) to <paramref name="intUpperBound"/> (exclusive).</returns>
        public static Task<TResult[]> ForAsync<TResult>(int intLowerBound, int intUpperBound, Func<int, Task<TResult>> funcCodeToRun, bool blnPooledArray, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<TResult[]>(token);
            int intReturnLength = intUpperBound - intLowerBound;
            if (intReturnLength <= 0)
                return Task.FromResult(blnPooledArray ? ArrayPool<TResult>.Shared.Rent(0) : Array.Empty<TResult>());
            return Inner();
            async Task<TResult[]> Inner()
            {
                int intCounter = 0;
                TResult[] aobjReturn = blnPooledArray ? ArrayPool<TResult>.Shared.Rent(intReturnLength) : new TResult[intReturnLength];
                try
                {
                    token.ThrowIfCancellationRequested();
                    int intBufferSize = Math.Min(intReturnLength, Utils.MaxParallelBatchSize);
                    List<Task<TResult>> lstBuffer = new List<Task<TResult>>(intBufferSize);
                    token.ThrowIfCancellationRequested();
                    int i = 0;
                    for (int j = intLowerBound; j < intUpperBound; ++j)
                    {
                        token.ThrowIfCancellationRequested();
                        lstBuffer.Add(funcCodeToRun(j));
                        if (++i == intBufferSize)
                        {
                            TResult[] aobjReturnInner = await Task.WhenAll(lstBuffer).ConfigureAwait(false);
                            for (int k = 0; k < i; ++k)
                                aobjReturn[intCounter++] = aobjReturnInner[k];
                            lstBuffer.Clear();
                            i = 0;
                        }
                    }
                    if (i > 0)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstBuffer).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        TResult[] aobjReturnInner = await Task.WhenAll(lstBuffer).ConfigureAwait(false);
                        for (int k = 0; k < i; ++k)
                            aobjReturn[intCounter++] = aobjReturnInner[k];
                    }
                    return aobjReturn;
                }
                catch when (blnPooledArray)
                {
                    ArrayPool<TResult>.Shared.Return(aobjReturn);
                    throw;
                }
            }
        }

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.For while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="intLowerBound">Starting value of the iterating variable (inclusive).</param>
        /// <param name="intUpperBound">Terminating value of the iterating variable (exclusive).</param>
        /// <param name="funcCodeToRunWithPotentialBreak">Code to run in parallel. CancellationTokenSource argument is for early termination of the loop, request it to cancel (but don't throw an exception) to make the loop terminate early.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>Array of the results of <paramref name="funcCodeToRun"/> when run from <paramref name="intLowerBound"/> (inclusive) to <paramref name="intUpperBound"/> (exclusive).</returns>
        public static Task<List<TResult>> ForAsync<TResult>(int intLowerBound, int intUpperBound, Func<int, CancellationTokenSource, Task<TResult>> funcCodeToRunWithPotentialBreak, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<List<TResult>>(token);
            int intReturnLength = intUpperBound - intLowerBound;
            if (intReturnLength <= 0)
                return Task.FromResult(new List<TResult>());
            return Inner();
            async Task<List<TResult>> Inner()
            {
                List<TResult> lstReturn = new List<TResult>(intReturnLength);
                token.ThrowIfCancellationRequested();
                using (CancellationTokenSource objBreakLoop = new CancellationTokenSource())
                {
                    CancellationToken objBreakToken = objBreakLoop.Token;
                    using (CancellationTokenTaskSource objBreakTokenTaskSource = new CancellationTokenTaskSource(objBreakToken))
                    {
                        Task objBreakTokenTask = objBreakTokenTaskSource.Task;
                        int intBufferSize = Math.Min(intReturnLength, Utils.MaxParallelBatchSize);
                        List<Task<TResult>> lstBuffer = new List<Task<TResult>>(intBufferSize);
                        token.ThrowIfCancellationRequested();
                        int i = 0;
                        for (int j = intLowerBound; j < intUpperBound; ++j)
                        {
                            token.ThrowIfCancellationRequested();
                            lstBuffer.Add(funcCodeToRunWithPotentialBreak(j, objBreakLoop));
                            if (++i == intBufferSize)
                            {
                                Task<TResult[]> tskEnsemble = Task.WhenAll(lstBuffer);
                                if (await Task.WhenAny(tskEnsemble, objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                {
                                    token.ThrowIfCancellationRequested();
                                    foreach (Task<TResult> tskLoop in lstBuffer)
                                    {
                                        if (!tskLoop.IsCanceled)
                                            lstReturn.Add(await tskLoop.ConfigureAwait(false));
                                    }
                                    return lstReturn;
                                }
                                else
                                {
                                    token.ThrowIfCancellationRequested();
                                    lstReturn.AddRange(await tskEnsemble.ConfigureAwait(false));
                                }
                                lstBuffer.Clear();
                                i = 0;
                            }
                        }
                        if (i > 0)
                        {
                            token.ThrowIfCancellationRequested();
                            if (await Task.WhenAny(Task.WhenAll(lstBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                            {
                                token.ThrowIfCancellationRequested();
                                foreach (Task<TResult> tskLoop in lstBuffer)
                                {
                                    if (!tskLoop.IsCanceled)
                                        lstReturn.Add(await tskLoop.ConfigureAwait(false));
                                }
                            }
                            else
                            {
                                foreach (Task<TResult> tskLoop in lstBuffer)
                                    lstReturn.Add(await tskLoop.ConfigureAwait(false));
                            }
                        }
                        return lstReturn;
                    }
                }
            }
        }
    }
}
