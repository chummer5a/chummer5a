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
            Task[] aobjTaskBuffer = ArrayPool<Task>.Shared.Rent(Utils.MaxParallelBatchSize);
            try
            {
                token.ThrowIfCancellationRequested();
                int i = 0;
                for (; i < Utils.MaxParallelBatchSize; ++i)
                {
                    aobjTaskBuffer[i] = Task.CompletedTask; // Ensures that none of these are null, just in case
                }
                i = 0;
                foreach (TSource objSource in lstItems)
                {
                    token.ThrowIfCancellationRequested();
                    aobjTaskBuffer[i++] = funcCodeToRun(objSource) ?? Task.CompletedTask;
                    if (i == Utils.MaxParallelBatchSize)
                    {
                        await Task.WhenAll(aobjTaskBuffer).ConfigureAwait(false);
                        i = 0;
                    }
                }
                if (i > 0)
                {
                    token.ThrowIfCancellationRequested();
                    for (int j = i; j < Utils.MaxParallelBatchSize; ++j)
                    {
                        aobjTaskBuffer[j] = Task.CompletedTask;
                    }
                    await Task.WhenAll(aobjTaskBuffer).ConfigureAwait(false);
                }

            }
            finally
            {
                ArrayPool<Task>.Shared.Return(aobjTaskBuffer);
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
            Task[] aobjTaskBuffer = ArrayPool<Task>.Shared.Rent(Utils.MaxParallelBatchSize);
            try
            {
                token.ThrowIfCancellationRequested();
                int i = 0;
                for (; i < Utils.MaxParallelBatchSize; ++i)
                {
                    aobjTaskBuffer[i] = Task.CompletedTask; // Ensures that none of these are null, just in case
                }
                i = 0;
                IEnumerator objEnumerator = lstItems.GetEnumerator();
                try
                {
                    token.ThrowIfCancellationRequested();
                    aobjTaskBuffer[i++] = funcCodeToRun(objEnumerator.Current) ?? Task.CompletedTask;
                    if (i == Utils.MaxParallelBatchSize)
                    {
                        await Task.WhenAll(aobjTaskBuffer).ConfigureAwait(false);
                        i = 0;
                    }
                }
                finally
                {
                    if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                        await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                    else if (objEnumerator is IDisposable objDisposable)
                        objDisposable.Dispose();
                }
                if (i > 0)
                {
                    token.ThrowIfCancellationRequested();
                    for (int j = i; j < Utils.MaxParallelBatchSize; ++j)
                    {
                        aobjTaskBuffer[j] = Task.CompletedTask;
                    }
                    await Task.WhenAll(aobjTaskBuffer).ConfigureAwait(false);
                }

            }
            finally
            {
                ArrayPool<Task>.Shared.Return(aobjTaskBuffer);
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
            Task[] aobjTaskBuffer = ArrayPool<Task>.Shared.Rent(Utils.MaxParallelBatchSize);
            try
            {
                token.ThrowIfCancellationRequested();
                int i = 0;
                for (; i < Utils.MaxParallelBatchSize; ++i)
                {
                    aobjTaskBuffer[i] = Task.CompletedTask; // Ensures that none of these are null, just in case
                }
                i = 0;
                IEnumerator<TSource> objEnumerator = await lstItems.GetEnumeratorAsync(token).ConfigureAwait(false);
                try
                {
                    while (objEnumerator.MoveNext())
                    {
                        token.ThrowIfCancellationRequested();
                        aobjTaskBuffer[i++] = funcCodeToRun(objEnumerator.Current) ?? Task.CompletedTask;
                        if (i == Utils.MaxParallelBatchSize)
                        {
                            await Task.WhenAll(aobjTaskBuffer).ConfigureAwait(false);
                            i = 0;
                        }
                    }

                    // Keep this last part inside the bloc before enumerator is disposed because we want to maintain the read lock on collections that have one until the parallel methods have completed
                    if (i > 0)
                    {
                        token.ThrowIfCancellationRequested();
                        for (int j = i; j < Utils.MaxParallelBatchSize; ++j)
                        {
                            aobjTaskBuffer[j] = Task.CompletedTask;
                        }
                        await Task.WhenAll(aobjTaskBuffer).ConfigureAwait(false);
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
            finally
            {
                ArrayPool<Task>.Shared.Return(aobjTaskBuffer);
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
            Task[] aobjTaskBuffer = ArrayPool<Task>.Shared.Rent(Utils.MaxParallelBatchSize);
            try
            {
                token.ThrowIfCancellationRequested();
                int i = 0;
                for (; i < Utils.MaxParallelBatchSize; ++i)
                {
                    aobjTaskBuffer[i] = Task.CompletedTask; // Ensures that none of these are null, just in case
                }
                i = 0;
                IEnumerator<TSource> objEnumerator = await lstItems.GetEnumeratorAsync(token).ConfigureAwait(false);
                try
                {
                    while (objEnumerator.MoveNext())
                    {
                        token.ThrowIfCancellationRequested();
                        aobjTaskBuffer[i++] = Task.Run(() => funcCodeToRun(objEnumerator.Current), token);
                        if (i == Utils.MaxParallelBatchSize)
                        {
                            await Task.WhenAll(aobjTaskBuffer).ConfigureAwait(false);
                            i = 0;
                        }
                    }

                    // Keep this last part inside the bloc before enumerator is disposed because we want to maintain the read lock on collections that have one until the parallel methods have completed
                    if (i > 0)
                    {
                        token.ThrowIfCancellationRequested();
                        for (int j = i; j < Utils.MaxParallelBatchSize; ++j)
                        {
                            aobjTaskBuffer[j] = Task.CompletedTask;
                        }
                        await Task.WhenAll(aobjTaskBuffer).ConfigureAwait(false);
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
            finally
            {
                ArrayPool<Task>.Shared.Return(aobjTaskBuffer);
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
            List<TResult> lstReturn = new List<TResult>(lstItems is IReadOnlyCollection<TSource> lstItemsCollection ? lstItemsCollection.Count : Utils.MaxParallelBatchSize);
            token.ThrowIfCancellationRequested();
            Task<TResult>[] aobjTaskBuffer = ArrayPool<Task<TResult>>.Shared.Rent(Utils.MaxParallelBatchSize);
            try
            {
                token.ThrowIfCancellationRequested();
                int i = 0;
                for (; i < Utils.MaxParallelBatchSize; ++i)
                {
                    aobjTaskBuffer[i] = Task.FromResult(default(TResult)); // Ensures that none of these are null, just in case
                }
                i = 0;
                foreach (TSource objSource in lstItems)
                {
                    token.ThrowIfCancellationRequested();
                    aobjTaskBuffer[i++] = funcCodeToRun(objSource) ?? Task.FromResult(default(TResult));
                    if (i == Utils.MaxParallelBatchSize)
                    {
                        lstReturn.AddRange(await Task.WhenAll(aobjTaskBuffer).ConfigureAwait(false));
                        i = 0;
                    }
                }
                if (i > 0)
                {
                    token.ThrowIfCancellationRequested();
                    for (int j = i; j < Utils.MaxParallelBatchSize; ++j)
                    {
                        aobjTaskBuffer[j] = Task.FromResult<TResult>(default);
                    }
                    await Task.WhenAll(aobjTaskBuffer).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    for (int j = 0; j <= i; ++j)
                    {
                        lstReturn.Add(await aobjTaskBuffer[j].ConfigureAwait(false));
                    }
                }

            }
            finally
            {
                ArrayPool<Task<TResult>>.Shared.Return(aobjTaskBuffer);
            }
            return lstReturn;
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
            List<TResult> lstReturn = new List<TResult>(lstItems is ICollection lstItemsCollection ? lstItemsCollection.Count : Utils.MaxParallelBatchSize);
            token.ThrowIfCancellationRequested();
            Task<TResult>[] aobjTaskBuffer = ArrayPool<Task<TResult>>.Shared.Rent(Utils.MaxParallelBatchSize);
            try
            {
                token.ThrowIfCancellationRequested();
                int i = 0;
                for (; i < Utils.MaxParallelBatchSize; ++i)
                {
                    aobjTaskBuffer[i] = Task.FromResult(default(TResult)); // Ensures that none of these are null, just in case
                }
                i = 0;
                IEnumerator objEnumerator = lstItems.GetEnumerator();
                try
                {
                    token.ThrowIfCancellationRequested();
                    aobjTaskBuffer[i++] = funcCodeToRun(objEnumerator.Current) ?? Task.FromResult(default(TResult));
                    if (i == Utils.MaxParallelBatchSize)
                    {
                        lstReturn.AddRange(await Task.WhenAll(aobjTaskBuffer).ConfigureAwait(false));
                        i = 0;
                    }
                }
                finally
                {
                    if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                        await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                    else if (objEnumerator is IDisposable objDisposable)
                        objDisposable.Dispose();
                }
                if (i > 0)
                {
                    token.ThrowIfCancellationRequested();
                    for (int j = i; j < Utils.MaxParallelBatchSize; ++j)
                    {
                        aobjTaskBuffer[j] = Task.FromResult<TResult>(default);
                    }
                    await Task.WhenAll(aobjTaskBuffer).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    for (int j = 0; j <= i; ++j)
                    {
                        lstReturn.Add(await aobjTaskBuffer[j].ConfigureAwait(false));
                    }
                }

            }
            finally
            {
                ArrayPool<Task<TResult>>.Shared.Return(aobjTaskBuffer);
            }
            return lstReturn;
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
            List<TResult> lstReturn = new List<TResult>(lstItems is IAsyncReadOnlyCollection<TSource> lstItemsCollection ? await lstItemsCollection.GetCountAsync(token).ConfigureAwait(false) : Utils.MaxParallelBatchSize);
            token.ThrowIfCancellationRequested();
            Task<TResult>[] aobjTaskBuffer = ArrayPool<Task<TResult>>.Shared.Rent(Utils.MaxParallelBatchSize);
            try
            {
                token.ThrowIfCancellationRequested();
                int i = 0;
                for (; i < Utils.MaxParallelBatchSize; ++i)
                {
                    aobjTaskBuffer[i] = Task.FromResult(default(TResult)); // Ensures that none of these are null, just in case
                }
                i = 0;
                IEnumerator<TSource> objEnumerator = await lstItems.GetEnumeratorAsync(token).ConfigureAwait(false);
                try
                {
                    while (objEnumerator.MoveNext())
                    {
                        token.ThrowIfCancellationRequested();
                        aobjTaskBuffer[i++] = funcCodeToRun(objEnumerator.Current) ?? Task.FromResult(default(TResult));
                        if (i == Utils.MaxParallelBatchSize)
                        {
                            lstReturn.AddRange(await Task.WhenAll(aobjTaskBuffer).ConfigureAwait(false));
                            i = 0;
                        }
                    }

                    // Keep this last part inside the bloc before enumerator is disposed because we want to maintain the read lock on collections that have one until the parallel methods have completed
                    if (i > 0)
                    {
                        token.ThrowIfCancellationRequested();
                        for (int j = i; j < Utils.MaxParallelBatchSize; ++j)
                        {
                            aobjTaskBuffer[j] = Task.FromResult<TResult>(default);
                        }
                        await Task.WhenAll(aobjTaskBuffer).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        for (int j = 0; j <= i; ++j)
                        {
                            lstReturn.Add(await aobjTaskBuffer[j].ConfigureAwait(false));
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
            finally
            {
                ArrayPool<Task<TResult>>.Shared.Return(aobjTaskBuffer);
            }
            return lstReturn;
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
            List<TResult> lstReturn = new List<TResult>(lstItems is IAsyncReadOnlyCollection<TSource> lstItemsCollection ? await lstItemsCollection.GetCountAsync(token).ConfigureAwait(false) : Utils.MaxParallelBatchSize);
            token.ThrowIfCancellationRequested();
            Task<TResult>[] aobjTaskBuffer = ArrayPool<Task<TResult>>.Shared.Rent(Utils.MaxParallelBatchSize);
            try
            {
                token.ThrowIfCancellationRequested();
                int i = 0;
                for (; i < Utils.MaxParallelBatchSize; ++i)
                {
                    aobjTaskBuffer[i] = Task.FromResult(default(TResult)); // Ensures that none of these are null, just in case
                }
                i = 0;
                IEnumerator<TSource> objEnumerator = await lstItems.GetEnumeratorAsync(token).ConfigureAwait(false);
                try
                {
                    while (objEnumerator.MoveNext())
                    {
                        token.ThrowIfCancellationRequested();
                        aobjTaskBuffer[i++] = Task.Run(() => funcCodeToRun(objEnumerator.Current), token) ?? Task.FromResult(default(TResult));
                        if (i == Utils.MaxParallelBatchSize)
                        {
                            lstReturn.AddRange(await Task.WhenAll(aobjTaskBuffer).ConfigureAwait(false));
                            i = 0;
                        }
                    }

                    // Keep this last part inside the bloc before enumerator is disposed because we want to maintain the read lock on collections that have one until the parallel methods have completed
                    if (i > 0)
                    {
                        token.ThrowIfCancellationRequested();
                        for (int j = i; j < Utils.MaxParallelBatchSize; ++j)
                        {
                            aobjTaskBuffer[j] = Task.FromResult<TResult>(default);
                        }
                        await Task.WhenAll(aobjTaskBuffer).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        for (int j = 0; j <= i; ++j)
                        {
                            lstReturn.Add(await aobjTaskBuffer[j].ConfigureAwait(false));
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
            finally
            {
                ArrayPool<Task<TResult>>.Shared.Return(aobjTaskBuffer);
            }
            return lstReturn;
        }

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.ForEach while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="lstItems">Enumerable supplying the source of items for the code we want to run in parallel.</param>
        /// <param name="funcCodeToRunWithPotentialBreak">Code to run in parallel. CancellationToken argument is for early termination of the loop, request it to cancel (but don't throw an exception) to make the loop terminate early.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachAsync<TSource>(IEnumerable<TSource> lstItems, Func<TSource, CancellationToken, Task> funcCodeToRunWithPotentialBreak, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (CancellationTokenSource objBreakLoop = new CancellationTokenSource())
            {
                CancellationToken objBreakToken = objBreakLoop.Token;
                Task objBreakTokenTask = objBreakToken.AsTask();
                using (CancellationTokenSource objJoinedSource = CancellationTokenSource.CreateLinkedTokenSource(objBreakToken, token))
                {
                    CancellationToken objJoinedToken = objBreakLoop.Token;
                    Task[] aobjTaskBuffer = ArrayPool<Task>.Shared.Rent(Utils.MaxParallelBatchSize);
                    try
                    {
                        objJoinedToken.ThrowIfCancellationRequested();
                        int i = 0;
                        for (; i < Utils.MaxParallelBatchSize; ++i)
                        {
                            aobjTaskBuffer[i] = Task.CompletedTask; // Ensures that none of these are null, just in case
                        }
                        i = 0;
                        foreach (TSource objSource in lstItems)
                        {
                            objJoinedToken.ThrowIfCancellationRequested();
                            aobjTaskBuffer[i++] = funcCodeToRunWithPotentialBreak(objSource, objJoinedToken) ?? Task.CompletedTask;
                            if (i == Utils.MaxParallelBatchSize)
                            {
                                if (await Task.WhenAny(Task.WhenAll(aobjTaskBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                    return;
                                i = 0;
                            }
                        }
                        if (i > 0)
                        {
                            objJoinedToken.ThrowIfCancellationRequested();
                            for (int j = i; j < Utils.MaxParallelBatchSize; ++j)
                            {
                                aobjTaskBuffer[j] = Task.CompletedTask;
                            }
                            if (await Task.WhenAny(Task.WhenAll(aobjTaskBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                return;
                        }

                    }
                    finally
                    {
                        ArrayPool<Task>.Shared.Return(aobjTaskBuffer);
                    }
                }
            }
        }

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.ForEach while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="lstItems">Enumerable supplying the source of items for the code we want to run in parallel.</param>
        /// <param name="funcCodeToRunWithPotentialBreak">Code to run in parallel. CancellationToken argument is for early termination of the loop, request it to cancel (but don't throw an exception) to make the loop terminate early.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachAsync(IEnumerable lstItems, Func<object, CancellationToken, Task> funcCodeToRunWithPotentialBreak, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (CancellationTokenSource objBreakLoop = new CancellationTokenSource())
            {
                CancellationToken objBreakToken = objBreakLoop.Token;
                Task objBreakTokenTask = objBreakToken.AsTask();
                using (CancellationTokenSource objJoinedSource = CancellationTokenSource.CreateLinkedTokenSource(objBreakToken, token))
                {
                    CancellationToken objJoinedToken = objBreakLoop.Token;
                    Task[] aobjTaskBuffer = ArrayPool<Task>.Shared.Rent(Utils.MaxParallelBatchSize);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        int i = 0;
                        for (; i < Utils.MaxParallelBatchSize; ++i)
                        {
                            aobjTaskBuffer[i] = Task.CompletedTask; // Ensures that none of these are null, just in case
                        }
                        i = 0;
                        IEnumerator objEnumerator = lstItems.GetEnumerator();
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            aobjTaskBuffer[i++] = funcCodeToRunWithPotentialBreak(objEnumerator.Current, objJoinedToken) ?? Task.CompletedTask;
                            if (i == Utils.MaxParallelBatchSize)
                            {
                                if (await Task.WhenAny(Task.WhenAll(aobjTaskBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                    return;
                                i = 0;
                            }
                        }
                        finally
                        {
                            if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                                await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                            else if (objEnumerator is IDisposable objDisposable)
                                objDisposable.Dispose();
                        }
                        if (i > 0)
                        {
                            token.ThrowIfCancellationRequested();
                            for (int j = i; j < Utils.MaxParallelBatchSize; ++j)
                            {
                                aobjTaskBuffer[j] = Task.CompletedTask;
                            }
                            if (await Task.WhenAny(Task.WhenAll(aobjTaskBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                return;
                        }

                    }
                    finally
                    {
                        ArrayPool<Task>.Shared.Return(aobjTaskBuffer);
                    }
                }
            }
        }

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.ForEach while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="lstItems">Enumerable supplying the source of items for the code we want to run in parallel.</param>
        /// <param name="funcCodeToRunWithPotentialBreak">Code to run in parallel. CancellationToken argument is for early termination of the loop, request it to cancel (but don't throw an exception) to make the loop terminate early.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachAsync<TSource>(IAsyncEnumerable<TSource> lstItems, Func<TSource, CancellationToken, Task> funcCodeToRunWithPotentialBreak, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (CancellationTokenSource objBreakLoop = new CancellationTokenSource())
            {
                CancellationToken objBreakToken = objBreakLoop.Token;
                Task objBreakTokenTask = objBreakToken.AsTask();
                using (CancellationTokenSource objJoinedSource = CancellationTokenSource.CreateLinkedTokenSource(objBreakToken, token))
                {
                    CancellationToken objJoinedToken = objBreakLoop.Token;
                    Task[] aobjTaskBuffer = ArrayPool<Task>.Shared.Rent(Utils.MaxParallelBatchSize);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        int i = 0;
                        for (; i < Utils.MaxParallelBatchSize; ++i)
                        {
                            aobjTaskBuffer[i] = Task.CompletedTask; // Ensures that none of these are null, just in case
                        }
                        i = 0;
                        IEnumerator<TSource> objEnumerator = await lstItems.GetEnumeratorAsync(token).ConfigureAwait(false);
                        try
                        {
                            while (objEnumerator.MoveNext())
                            {
                                token.ThrowIfCancellationRequested();
                                aobjTaskBuffer[i++] = funcCodeToRunWithPotentialBreak(objEnumerator.Current, objJoinedToken) ?? Task.CompletedTask;
                                if (i == Utils.MaxParallelBatchSize)
                                {
                                    if (await Task.WhenAny(Task.WhenAll(aobjTaskBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                        return;
                                    i = 0;
                                }
                            }

                            // Keep this last part inside the bloc before enumerator is disposed because we want to maintain the read lock on collections that have one until the parallel methods have completed
                            if (i > 0)
                            {
                                token.ThrowIfCancellationRequested();
                                for (int j = i; j < Utils.MaxParallelBatchSize; ++j)
                                {
                                    aobjTaskBuffer[j] = Task.CompletedTask;
                                }
                                if (await Task.WhenAny(Task.WhenAll(aobjTaskBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
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
                    finally
                    {
                        ArrayPool<Task>.Shared.Return(aobjTaskBuffer);
                    }
                }
            }
        }

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.ForEach while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="lstItems">Enumerable supplying the source of items for the code we want to run in parallel.</param>
        /// <param name="funcCodeToRunWithPotentialBreak">Code to run in parallel. CancellationToken argument is for early termination of the loop, request it to cancel (but don't throw an exception) to make the loop terminate early.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachAsync<TSource>(IAsyncEnumerable<TSource> lstItems, Action<TSource, CancellationToken> funcCodeToRunWithPotentialBreak, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (CancellationTokenSource objBreakLoop = new CancellationTokenSource())
            {
                CancellationToken objBreakToken = objBreakLoop.Token;
                Task objBreakTokenTask = objBreakToken.AsTask();
                using (CancellationTokenSource objJoinedSource = CancellationTokenSource.CreateLinkedTokenSource(objBreakToken, token))
                {
                    CancellationToken objJoinedToken = objBreakLoop.Token;
                    Task[] aobjTaskBuffer = ArrayPool<Task>.Shared.Rent(Utils.MaxParallelBatchSize);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        int i = 0;
                        for (; i < Utils.MaxParallelBatchSize; ++i)
                        {
                            aobjTaskBuffer[i] = Task.CompletedTask; // Ensures that none of these are null, just in case
                        }
                        i = 0;
                        IEnumerator<TSource> objEnumerator = await lstItems.GetEnumeratorAsync(token).ConfigureAwait(false);
                        try
                        {
                            while (objEnumerator.MoveNext())
                            {
                                token.ThrowIfCancellationRequested();
                                aobjTaskBuffer[i++] = Task.Run(() => funcCodeToRunWithPotentialBreak(objEnumerator.Current, objJoinedToken), objJoinedToken);
                                if (i == Utils.MaxParallelBatchSize)
                                {
                                    if (await Task.WhenAny(Task.WhenAll(aobjTaskBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                        return;
                                    i = 0;
                                }
                            }

                            // Keep this last part inside the bloc before enumerator is disposed because we want to maintain the read lock on collections that have one until the parallel methods have completed
                            if (i > 0)
                            {
                                token.ThrowIfCancellationRequested();
                                for (int j = i; j < Utils.MaxParallelBatchSize; ++j)
                                {
                                    aobjTaskBuffer[j] = Task.CompletedTask;
                                }
                                if (await Task.WhenAny(Task.WhenAll(aobjTaskBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
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
                    finally
                    {
                        ArrayPool<Task>.Shared.Return(aobjTaskBuffer);
                    }
                }
            }
        }

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.ForEach while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="lstItems">Enumerable supplying the source of items for the code we want to run in parallel.</param>
        /// <param name="funcCodeToRunWithPotentialBreak">Code to run in parallel. CancellationToken argument is for early termination of the loop, request it to cancel (but don't throw an exception) to make the loop terminate early.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>List of the results of <paramref name="funcCodeToRun"/> when run over <paramref name="lstItems"/>.</returns>
        public static async Task<List<TResult>> ForEachAsync<TSource, TResult>(IEnumerable<TSource> lstItems, Func<TSource, CancellationToken, Task<TResult>> funcCodeToRunWithPotentialBreak, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<TResult> lstReturn = new List<TResult>(lstItems is IReadOnlyCollection<TSource> lstItemsCollection ? lstItemsCollection.Count : Utils.MaxParallelBatchSize);
            token.ThrowIfCancellationRequested();
            using (CancellationTokenSource objBreakLoop = new CancellationTokenSource())
            {
                CancellationToken objBreakToken = objBreakLoop.Token;
                Task objBreakTokenTask = objBreakToken.AsTask();
                using (CancellationTokenSource objJoinedSource = CancellationTokenSource.CreateLinkedTokenSource(objBreakToken, token))
                {
                    CancellationToken objJoinedToken = objBreakLoop.Token;
                    Task<TResult>[] aobjTaskBuffer = ArrayPool<Task<TResult>>.Shared.Rent(Utils.MaxParallelBatchSize);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        int i = 0;
                        for (; i < Utils.MaxParallelBatchSize; ++i)
                        {
                            aobjTaskBuffer[i] = Task.FromResult(default(TResult)); // Ensures that none of these are null, just in case
                        }
                        i = 0;
                        foreach (TSource objSource in lstItems)
                        {
                            token.ThrowIfCancellationRequested();
                            aobjTaskBuffer[i++] = funcCodeToRunWithPotentialBreak(objSource, objJoinedToken) ?? Task.FromResult(default(TResult));
                            if (i == Utils.MaxParallelBatchSize)
                            {
                                Task<TResult[]> tskEnsemble = Task.WhenAll(aobjTaskBuffer);
                                if (await Task.WhenAny(tskEnsemble, objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                {
                                    token.ThrowIfCancellationRequested();
                                    for (int j = 0; j < i; ++j)
                                    {
                                        Task<TResult> tskLoop = aobjTaskBuffer[j];
                                        if (!tskLoop.IsCanceled)
                                            lstReturn.Add(await tskLoop.ConfigureAwait(false));
                                    }
                                }
                                else
                                {
                                    token.ThrowIfCancellationRequested();
                                    lstReturn.AddRange(await tskEnsemble.ConfigureAwait(false));
                                }
                                i = 0;
                            }
                        }
                        if (i > 0)
                        {
                            token.ThrowIfCancellationRequested();
                            for (int j = i; j < Utils.MaxParallelBatchSize; ++j)
                            {
                                aobjTaskBuffer[j] = Task.FromResult<TResult>(default);
                            }
                            if (await Task.WhenAny(Task.WhenAll(aobjTaskBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                            {
                                token.ThrowIfCancellationRequested();
                                for (int j = 0; j < i; ++j)
                                {
                                    Task<TResult> tskLoop = aobjTaskBuffer[j];
                                    if (!tskLoop.IsCanceled)
                                        lstReturn.Add(await tskLoop.ConfigureAwait(false));
                                }
                            }
                            else
                            {
                                token.ThrowIfCancellationRequested();
                                for (int j = 0; j < i; ++j)
                                {
                                    lstReturn.Add(await aobjTaskBuffer[j].ConfigureAwait(false));
                                }
                            }
                        }

                    }
                    finally
                    {
                        ArrayPool<Task<TResult>>.Shared.Return(aobjTaskBuffer);
                    }
                    return lstReturn;
                }
            }
        }

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.ForEach while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="lstItems">Enumerable supplying the source of items for the code we want to run in parallel.</param>
        /// <param name="funcCodeToRunWithPotentialBreak">Code to run in parallel. CancellationToken argument is for early termination of the loop, request it to cancel (but don't throw an exception) to make the loop terminate early.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>List of the results of <paramref name="funcCodeToRun"/> when run over <paramref name="lstItems"/>.</returns>
        public static async Task<List<TResult>> ForEachAsync<TResult>(IEnumerable lstItems, Func<object, CancellationToken, Task<TResult>> funcCodeToRunWithPotentialBreak, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<TResult> lstReturn = new List<TResult>(lstItems is ICollection lstItemsCollection ? lstItemsCollection.Count : Utils.MaxParallelBatchSize);
            token.ThrowIfCancellationRequested();
            using (CancellationTokenSource objBreakLoop = new CancellationTokenSource())
            {
                CancellationToken objBreakToken = objBreakLoop.Token;
                Task objBreakTokenTask = objBreakToken.AsTask();
                using (CancellationTokenSource objJoinedSource = CancellationTokenSource.CreateLinkedTokenSource(objBreakToken, token))
                {
                    CancellationToken objJoinedToken = objBreakLoop.Token;
                    Task<TResult>[] aobjTaskBuffer = ArrayPool<Task<TResult>>.Shared.Rent(Utils.MaxParallelBatchSize);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        int i = 0;
                        for (; i < Utils.MaxParallelBatchSize; ++i)
                        {
                            aobjTaskBuffer[i] = Task.FromResult(default(TResult)); // Ensures that none of these are null, just in case
                        }
                        i = 0;
                        IEnumerator objEnumerator = lstItems.GetEnumerator();
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            aobjTaskBuffer[i++] = funcCodeToRunWithPotentialBreak(objEnumerator.Current, objJoinedToken) ?? Task.FromResult(default(TResult));
                            if (i == Utils.MaxParallelBatchSize)
                            {
                                Task<TResult[]> tskEnsemble = Task.WhenAll(aobjTaskBuffer);
                                if (await Task.WhenAny(tskEnsemble, objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                {
                                    token.ThrowIfCancellationRequested();
                                    for (int j = 0; j < i; ++j)
                                    {
                                        Task<TResult> tskLoop = aobjTaskBuffer[j];
                                        if (!tskLoop.IsCanceled)
                                            lstReturn.Add(await tskLoop.ConfigureAwait(false));
                                    }
                                }
                                else
                                {
                                    token.ThrowIfCancellationRequested();
                                    lstReturn.AddRange(await tskEnsemble.ConfigureAwait(false));
                                }
                                i = 0;
                            }
                        }
                        finally
                        {
                            if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                                await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                            else if (objEnumerator is IDisposable objDisposable)
                                objDisposable.Dispose();
                        }
                        if (i > 0)
                        {
                            token.ThrowIfCancellationRequested();
                            for (int j = i; j < Utils.MaxParallelBatchSize; ++j)
                            {
                                aobjTaskBuffer[j] = Task.FromResult<TResult>(default);
                            }
                            if (await Task.WhenAny(Task.WhenAll(aobjTaskBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                            {
                                token.ThrowIfCancellationRequested();
                                for (int j = 0; j < i; ++j)
                                {
                                    Task<TResult> tskLoop = aobjTaskBuffer[j];
                                    if (!tskLoop.IsCanceled)
                                        lstReturn.Add(await tskLoop.ConfigureAwait(false));
                                }
                            }
                            else
                            {
                                token.ThrowIfCancellationRequested();
                                for (int j = 0; j < i; ++j)
                                {
                                    lstReturn.Add(await aobjTaskBuffer[j].ConfigureAwait(false));
                                }
                            }
                        }

                    }
                    finally
                    {
                        ArrayPool<Task<TResult>>.Shared.Return(aobjTaskBuffer);
                    }
                    return lstReturn;
                }
            }
        }

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.ForEach while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="lstItems">Enumerable supplying the source of items for the code we want to run in parallel.</param>
        /// <param name="funcCodeToRunWithPotentialBreak">Code to run in parallel. CancellationToken argument is for early termination of the loop, request it to cancel (but don't throw an exception) to make the loop terminate early.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>List of the results of <paramref name="funcCodeToRun"/> when run over <paramref name="lstItems"/>.</returns>
        public static async Task<List<TResult>> ForEachAsync<TSource, TResult>(IAsyncEnumerable<TSource> lstItems, Func<TSource, CancellationToken, Task<TResult>> funcCodeToRunWithPotentialBreak, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<TResult> lstReturn = new List<TResult>(lstItems is IAsyncReadOnlyCollection<TSource> lstItemsCollection ? await lstItemsCollection.GetCountAsync(token).ConfigureAwait(false) : Utils.MaxParallelBatchSize);
            token.ThrowIfCancellationRequested();
            using (CancellationTokenSource objBreakLoop = new CancellationTokenSource())
            {
                CancellationToken objBreakToken = objBreakLoop.Token;
                Task objBreakTokenTask = objBreakToken.AsTask();
                using (CancellationTokenSource objJoinedSource = CancellationTokenSource.CreateLinkedTokenSource(objBreakToken, token))
                {
                    CancellationToken objJoinedToken = objBreakLoop.Token;
                    Task<TResult>[] aobjTaskBuffer = ArrayPool<Task<TResult>>.Shared.Rent(Utils.MaxParallelBatchSize);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        int i = 0;
                        for (; i < Utils.MaxParallelBatchSize; ++i)
                        {
                            aobjTaskBuffer[i] = Task.FromResult(default(TResult)); // Ensures that none of these are null, just in case
                        }
                        i = 0;
                        IEnumerator<TSource> objEnumerator = await lstItems.GetEnumeratorAsync(token).ConfigureAwait(false);
                        try
                        {
                            while (objEnumerator.MoveNext())
                            {
                                token.ThrowIfCancellationRequested();
                                aobjTaskBuffer[i++] = funcCodeToRunWithPotentialBreak(objEnumerator.Current, objJoinedToken) ?? Task.FromResult(default(TResult));
                                if (i == Utils.MaxParallelBatchSize)
                                {
                                    Task<TResult[]> tskEnsemble = Task.WhenAll(aobjTaskBuffer);
                                    if (await Task.WhenAny(tskEnsemble, objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                    {
                                        token.ThrowIfCancellationRequested();
                                        for (int j = 0; j < i; ++j)
                                        {
                                            Task<TResult> tskLoop = aobjTaskBuffer[j];
                                            if (!tskLoop.IsCanceled)
                                                lstReturn.Add(await tskLoop.ConfigureAwait(false));
                                        }
                                    }
                                    else
                                    {
                                        token.ThrowIfCancellationRequested();
                                        lstReturn.AddRange(await tskEnsemble.ConfigureAwait(false));
                                    }
                                    i = 0;
                                }
                            }

                            // Keep this last part inside the bloc before enumerator is disposed because we want to maintain the read lock on collections that have one until the parallel methods have completed
                            if (i > 0)
                            {
                                token.ThrowIfCancellationRequested();
                                for (int j = i; j < Utils.MaxParallelBatchSize; ++j)
                                {
                                    aobjTaskBuffer[j] = Task.FromResult<TResult>(default);
                                }
                                if (await Task.WhenAny(Task.WhenAll(aobjTaskBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                {
                                    token.ThrowIfCancellationRequested();
                                    for (int j = 0; j < i; ++j)
                                    {
                                        Task<TResult> tskLoop = aobjTaskBuffer[j];
                                        if (!tskLoop.IsCanceled)
                                            lstReturn.Add(await tskLoop.ConfigureAwait(false));
                                    }
                                }
                                else
                                {
                                    token.ThrowIfCancellationRequested();
                                    for (int j = 0; j < i; ++j)
                                    {
                                        lstReturn.Add(await aobjTaskBuffer[j].ConfigureAwait(false));
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
                    finally
                    {
                        ArrayPool<Task<TResult>>.Shared.Return(aobjTaskBuffer);
                    }
                    return lstReturn;
                }
            }
        }

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.ForEach while respecting the max parallel batch size we have set in Utils.
        /// </summary>
        /// <param name="lstItems">Enumerable supplying the source of items for the code we want to run in parallel.</param>
        /// <param name="funcCodeToRunWithPotentialBreak">Code to run in parallel. CancellationToken argument is for early termination of the loop, request it to cancel (but don't throw an exception) to make the loop terminate early.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>List of the results of <paramref name="funcCodeToRun"/> when run over <paramref name="lstItems"/>.</returns>
        public static async Task<List<TResult>> ForEachAsync<TSource, TResult>(IAsyncEnumerable<TSource> lstItems, Func<TSource, CancellationToken, TResult> funcCodeToRunWithPotentialBreak, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<TResult> lstReturn = new List<TResult>(lstItems is IAsyncReadOnlyCollection<TSource> lstItemsCollection ? await lstItemsCollection.GetCountAsync(token).ConfigureAwait(false) : Utils.MaxParallelBatchSize);
            token.ThrowIfCancellationRequested();
            using (CancellationTokenSource objBreakLoop = new CancellationTokenSource())
            {
                CancellationToken objBreakToken = objBreakLoop.Token;
                Task objBreakTokenTask = objBreakToken.AsTask();
                using (CancellationTokenSource objJoinedSource = CancellationTokenSource.CreateLinkedTokenSource(objBreakToken, token))
                {
                    CancellationToken objJoinedToken = objBreakLoop.Token;
                    Task<TResult>[] aobjTaskBuffer = ArrayPool<Task<TResult>>.Shared.Rent(Utils.MaxParallelBatchSize);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        int i = 0;
                        for (; i < Utils.MaxParallelBatchSize; ++i)
                        {
                            aobjTaskBuffer[i] = Task.FromResult(default(TResult)); // Ensures that none of these are null, just in case
                        }
                        i = 0;
                        IEnumerator<TSource> objEnumerator = await lstItems.GetEnumeratorAsync(token).ConfigureAwait(false);
                        try
                        {
                            while (objEnumerator.MoveNext())
                            {
                                token.ThrowIfCancellationRequested();
                                aobjTaskBuffer[i++] = Task.Run(() => funcCodeToRunWithPotentialBreak(objEnumerator.Current, objJoinedToken), objJoinedToken);
                                if (i == Utils.MaxParallelBatchSize)
                                {
                                    Task<TResult[]> tskEnsemble = Task.WhenAll(aobjTaskBuffer);
                                    if (await Task.WhenAny(tskEnsemble, objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                    {
                                        token.ThrowIfCancellationRequested();
                                        for (int j = 0; j < i; ++j)
                                        {
                                            Task<TResult> tskLoop = aobjTaskBuffer[j];
                                            if (!tskLoop.IsCanceled)
                                                lstReturn.Add(await tskLoop.ConfigureAwait(false));
                                        }
                                    }
                                    else
                                    {
                                        token.ThrowIfCancellationRequested();
                                        lstReturn.AddRange(await tskEnsemble.ConfigureAwait(false));
                                    }
                                    i = 0;
                                }
                            }

                            // Keep this last part inside the bloc before enumerator is disposed because we want to maintain the read lock on collections that have one until the parallel methods have completed
                            if (i > 0)
                            {
                                token.ThrowIfCancellationRequested();
                                for (int j = i; j < Utils.MaxParallelBatchSize; ++j)
                                {
                                    aobjTaskBuffer[j] = Task.FromResult<TResult>(default);
                                }
                                if (await Task.WhenAny(Task.WhenAll(aobjTaskBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                {
                                    token.ThrowIfCancellationRequested();
                                    for (int j = 0; j < i; ++j)
                                    {
                                        Task<TResult> tskLoop = aobjTaskBuffer[j];
                                        if (!tskLoop.IsCanceled)
                                            lstReturn.Add(await tskLoop.ConfigureAwait(false));
                                    }
                                }
                                else
                                {
                                    token.ThrowIfCancellationRequested();
                                    for (int j = 0; j < i; ++j)
                                    {
                                        lstReturn.Add(await aobjTaskBuffer[j].ConfigureAwait(false));
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
                    finally
                    {
                        ArrayPool<Task<TResult>>.Shared.Return(aobjTaskBuffer);
                    }
                }
            }
            return lstReturn;
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
            if (intLowerBound >= intUpperBound)
                return Task.CompletedTask;
            return Inner();
            async Task Inner()
            {
                Task[] aobjTaskBuffer = ArrayPool<Task>.Shared.Rent(Utils.MaxParallelBatchSize);
                try
                {
                    token.ThrowIfCancellationRequested();
                    int i = 0;
                    for (; i < Utils.MaxParallelBatchSize; ++i)
                    {
                        aobjTaskBuffer[i] = Task.CompletedTask; // Ensures that none of these are null, just in case
                    }
                    i = 0;
                    for (int j = intLowerBound; j < intUpperBound; ++j)
                    {
                        token.ThrowIfCancellationRequested();
                        aobjTaskBuffer[i++] = funcCodeToRun(j) ?? Task.CompletedTask;
                        if (i == Utils.MaxParallelBatchSize)
                        {
                            await Task.WhenAll(aobjTaskBuffer).ConfigureAwait(false);
                            i = 0;
                        }
                    }
                    if (i > 0)
                    {
                        token.ThrowIfCancellationRequested();
                        for (int j = i; j < Utils.MaxParallelBatchSize; ++j)
                        {
                            aobjTaskBuffer[j] = Task.CompletedTask;
                        }
                        await Task.WhenAll(aobjTaskBuffer).ConfigureAwait(false);
                    }

                }
                finally
                {
                    ArrayPool<Task>.Shared.Return(aobjTaskBuffer);
                }
            }
        }

        /// <summary>
        /// Syntactic sugar to process a batch of asynchronous method calls in parallel similar to Parallel.For while respecting the max parallel batch size we have set in Utils
        /// </summary>
        /// <param name="intLowerBound">Starting value of the iterating variable (inclusive).</param>
        /// <param name="intUpperBound">Terminating value of the iterating variable (exclusive).</param>
        /// <param name="funcCodeToRunWithPotentialBreak">Code to run in parallel. CancellationToken argument is for early termination of the loop, request it to cancel (but don't throw an exception) to make the loop terminate early.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Task ForAsync(int intLowerBound, int intUpperBound, Func<int, CancellationToken, Task> funcCodeToRunWithPotentialBreak, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (intLowerBound >= intUpperBound)
                return Task.CompletedTask;
            return Inner();
            async Task Inner()
            {
                using (CancellationTokenSource objBreakLoop = new CancellationTokenSource())
                {
                    CancellationToken objBreakToken = objBreakLoop.Token;
                    Task objBreakTokenTask = objBreakToken.AsTask();
                    using (CancellationTokenSource objJoinedSource = CancellationTokenSource.CreateLinkedTokenSource(objBreakToken, token))
                    {
                        CancellationToken objJoinedToken = objBreakLoop.Token;
                        Task[] aobjTaskBuffer = ArrayPool<Task>.Shared.Rent(Utils.MaxParallelBatchSize);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            int i = 0;
                            for (; i < Utils.MaxParallelBatchSize; ++i)
                            {
                                aobjTaskBuffer[i] = Task.CompletedTask; // Ensures that none of these are null, just in case
                            }
                            i = 0;
                            for (int j = intLowerBound; j < intUpperBound; ++j)
                            {
                                token.ThrowIfCancellationRequested();
                                aobjTaskBuffer[i++] = funcCodeToRunWithPotentialBreak(j, objJoinedToken) ?? Task.CompletedTask;
                                if (i == Utils.MaxParallelBatchSize)
                                {
                                    if (await Task.WhenAny(Task.WhenAll(aobjTaskBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                        return;
                                    i = 0;
                                }
                            }
                            if (i > 0)
                            {
                                token.ThrowIfCancellationRequested();
                                for (int j = i; j < Utils.MaxParallelBatchSize; ++j)
                                {
                                    aobjTaskBuffer[j] = Task.CompletedTask;
                                }
                                if (await Task.WhenAny(Task.WhenAll(aobjTaskBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                    return;
                            }

                        }
                        finally
                        {
                            ArrayPool<Task>.Shared.Return(aobjTaskBuffer);
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
                    Task<TResult>[] aobjTaskBuffer = ArrayPool<Task<TResult>>.Shared.Rent(Utils.MaxParallelBatchSize);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        int i = 0;
                        for (; i < Utils.MaxParallelBatchSize; ++i)
                        {
                            aobjTaskBuffer[i] = Task.FromResult(default(TResult)); // Ensures that none of these are null, just in case
                        }
                        i = 0;
                        for (int j = intLowerBound; j < intUpperBound; ++j)
                        {
                            token.ThrowIfCancellationRequested();
                            aobjTaskBuffer[i++] = funcCodeToRun(j) ?? Task.FromResult(default(TResult));
                            if (i == Utils.MaxParallelBatchSize)
                            {
                                TResult[] aobjReturnInner = await Task.WhenAll(aobjTaskBuffer).ConfigureAwait(false);
                                for (int k = 0; k < i; ++k)
                                    aobjReturn[intCounter++] = aobjReturnInner[k];
                                i = 0;
                            }
                        }
                        if (i > 0)
                        {
                            token.ThrowIfCancellationRequested();
                            for (int j = i; j < Utils.MaxParallelBatchSize; ++j)
                            {
                                aobjTaskBuffer[j] = Task.FromResult<TResult>(default);
                            }
                            await Task.WhenAll(aobjTaskBuffer).ConfigureAwait(false);
                            token.ThrowIfCancellationRequested();
                            TResult[] aobjReturnInner = await Task.WhenAll(aobjTaskBuffer).ConfigureAwait(false);
                            for (int k = 0; k < i; ++k)
                                aobjReturn[intCounter++] = aobjReturnInner[k];
                        }

                    }
                    finally
                    {
                        ArrayPool<Task<TResult>>.Shared.Return(aobjTaskBuffer);
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
        /// <param name="funcCodeToRunWithPotentialBreak">Code to run in parallel. CancellationToken argument is for early termination of the loop, request it to cancel (but don't throw an exception) to make the loop terminate early.</param>
        /// <param name="blnPooledArray">Whether the returned array should be one taken from ArrayPool.Shared</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>Array of the results of <paramref name="funcCodeToRun"/> when run from <paramref name="intLowerBound"/> (inclusive) to <paramref name="intUpperBound"/> (exclusive).</returns>
        public static Task<List<TResult>> ForAsync<TResult>(int intLowerBound, int intUpperBound, Func<int, CancellationToken, Task<TResult>> funcCodeToRunWithPotentialBreak, CancellationToken token = default)
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
                    Task objBreakTokenTask = objBreakToken.AsTask();
                    using (CancellationTokenSource objJoinedSource = CancellationTokenSource.CreateLinkedTokenSource(objBreakToken, token))
                    {
                        CancellationToken objJoinedToken = objBreakLoop.Token;
                        Task<TResult>[] aobjTaskBuffer = ArrayPool<Task<TResult>>.Shared.Rent(Utils.MaxParallelBatchSize);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            int i = 0;
                            for (; i < Utils.MaxParallelBatchSize; ++i)
                            {
                                aobjTaskBuffer[i] = Task.FromResult(default(TResult)); // Ensures that none of these are null, just in case
                            }
                            i = 0;
                            for (int j = intLowerBound; j < intUpperBound; ++j)
                            {
                                token.ThrowIfCancellationRequested();
                                aobjTaskBuffer[i++] = funcCodeToRunWithPotentialBreak(j, objJoinedToken) ?? Task.FromResult(default(TResult));
                                if (i == Utils.MaxParallelBatchSize)
                                {
                                    Task<TResult[]> tskEnsemble = Task.WhenAll(aobjTaskBuffer);
                                    if (await Task.WhenAny(tskEnsemble, objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                    {
                                        token.ThrowIfCancellationRequested();
                                        for (int k = 0; k < i; ++k)
                                        {
                                            Task<TResult> tskLoop = aobjTaskBuffer[k];
                                            if (!tskLoop.IsCanceled)
                                                lstReturn.Add(await tskLoop.ConfigureAwait(false));
                                        }
                                    }
                                    else
                                    {
                                        token.ThrowIfCancellationRequested();
                                        lstReturn.AddRange(await tskEnsemble.ConfigureAwait(false));
                                    }
                                    i = 0;
                                }
                            }
                            if (i > 0)
                            {
                                token.ThrowIfCancellationRequested();
                                for (int j = i; j < Utils.MaxParallelBatchSize; ++j)
                                {
                                    aobjTaskBuffer[j] = Task.FromResult<TResult>(default);
                                }
                                if (await Task.WhenAny(Task.WhenAll(aobjTaskBuffer), objBreakTokenTask).ConfigureAwait(false) == objBreakTokenTask)
                                {
                                    token.ThrowIfCancellationRequested();
                                    for (int j = 0; j < i; ++j)
                                    {
                                        Task<TResult> tskLoop = aobjTaskBuffer[j];
                                        if (!tskLoop.IsCanceled)
                                            lstReturn.Add(await tskLoop.ConfigureAwait(false));
                                    }
                                }
                                else
                                {
                                    for (int j = 0; j < i; ++j)
                                    {
                                        lstReturn.Add(await aobjTaskBuffer[j].ConfigureAwait(false));
                                    }
                                }
                            }

                        }
                        finally
                        {
                            ArrayPool<Task<TResult>>.Shared.Return(aobjTaskBuffer);
                        }
                        return lstReturn;
                    }
                }
            }
        }
    }
}
