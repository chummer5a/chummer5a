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

using Chummer.Annotations;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    internal static class AsyncEnumerableExtensions
    {
        /// <summary>
        /// Syntactic sugar for collecting the elements of a <see cref="IAsyncEnumerable{T}"/> into a list.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static async Task<List<T>> ToListAsync<T>(IAsyncEnumerable<T> objEnumerable, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<T> lstReturn = new List<T>();
            await foreach (T objItem in objEnumerable.ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                lstReturn.Add(objItem);
            }
            return lstReturn;
        }

        internal static async Task<bool> AnyAsync<T>(IAsyncEnumerable<T> objEnumerable, Func<T, bool> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await foreach (T objItem in objEnumerable.ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (funcPredicate(objItem))
                    return true;
            }
            return false;
        }

        internal static async Task<bool> AnyAsync<T>(IAsyncEnumerable<T> objEnumerable, Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await foreach (T objItem in objEnumerable.ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (await funcPredicate(objItem).ConfigureAwait(false))
                    return true;
            }
            return false;
        }

        internal static async Task<bool> AnyAsync<T>(IAsyncEnumerable<T> objEnumerable, Func<T, CancellationToken, bool> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await foreach (T objItem in objEnumerable.ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (funcPredicate(objItem, token))
                    return true;
            }
            return false;
        }

        internal static async Task<bool> AnyAsync<T>(IAsyncEnumerable<T> objEnumerable, Func<T, CancellationToken, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await foreach (T objItem in objEnumerable.ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (await funcPredicate(objItem, token).ConfigureAwait(false))
                    return true;
            }
            return false;
        }

        internal static async Task<bool> AllAsync<T>(IAsyncEnumerable<T> objEnumerable, Func<T, bool> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await foreach (T objItem in objEnumerable.ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (!funcPredicate(objItem))
                    return false;
            }
            return true;
        }

        internal static async Task<bool> AllAsync<T>(IAsyncEnumerable<T> objEnumerable, Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await foreach (T objItem in objEnumerable.ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (!await funcPredicate(objItem).ConfigureAwait(false))
                    return false;
            }
            return true;
        }

        internal static async Task<bool> AllAsync<T>(IAsyncEnumerable<T> objEnumerable, Func<T, CancellationToken, bool> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await foreach (T objItem in objEnumerable.ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (!funcPredicate(objItem, token))
                    return false;
            }
            return true;
        }

        internal static async Task<bool> AllAsync<T>(IAsyncEnumerable<T> objEnumerable, Func<T, CancellationToken, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await foreach (T objItem in objEnumerable.ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (!await funcPredicate(objItem, token).ConfigureAwait(false))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Perform some function to select all elements from an async-capable enumerable or collection and pass it on.
        /// </summary>
        /// <param name="objEnumerable">Enumerable from which to select items to pass on.</param>
        /// <param name="funcSelector">Function to select what gets passed on to the output.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        internal static async IAsyncEnumerable<T2> SelectAsync<T1, T2>(this IAsyncEnumerable<T1> objEnumerable, [NotNull] Func<T1, Task<T2>> funcSelector, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objEnumerable is IEnumerableWithAsync<T1> objEnumerableCast)
            {
                IEnumerator<T1> objEnumerator = await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false);
                try
                {
                    while (objEnumerator.MoveNext())
                    {
                        token.ThrowIfCancellationRequested();
                        T2 objReturn = await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false);
                        if (objReturn != null)
                            yield return objReturn;
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
            else
            {
                await foreach (T1 objItem in objEnumerable.ConfigureAwait(false))
                {
                    token.ThrowIfCancellationRequested();
                    T2 objReturn = await funcSelector.Invoke(objItem).ConfigureAwait(false);
                    if (objReturn != null)
                        yield return objReturn;
                }
            }
        }

        /// <summary>
        /// Perform some function to select all elements from an async-capable enumerable or collection and pass it on.
        /// </summary>
        /// <param name="objEnumerable">Enumerable from which to select items to pass on.</param>
        /// <param name="funcSelector">Function to select what gets passed on to the output.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        internal static async IAsyncEnumerable<T2> SelectAsync<T1, T2>(this IAsyncEnumerable<T1> objEnumerable, [NotNull] Func<T1, T2> funcSelector, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objEnumerable is IEnumerableWithAsync<T1> objEnumerableCast)
            {
                IEnumerator<T1> objEnumerator = await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false);
                try
                {
                    while (objEnumerator.MoveNext())
                    {
                        token.ThrowIfCancellationRequested();
                        T2 objReturn = funcSelector.Invoke(objEnumerator.Current);
                        if (objReturn != null)
                            yield return objReturn;
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
            else
            {
                await foreach (T1 objItem in objEnumerable.ConfigureAwait(false))
                {
                    token.ThrowIfCancellationRequested();
                    T2 objReturn = funcSelector.Invoke(objItem);
                    if (objReturn != null)
                        yield return objReturn;
                }
            }
        }

        /// <summary>
        /// Perform some function to select all elements from an async-capable enumerable or collection and pass it on.
        /// </summary>
        /// <param name="objEnumerable">Enumerable from which to select items to pass on.</param>
        /// <param name="funcSelector">Function to select what gets passed on to the output.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        internal static async IAsyncEnumerable<T2> SelectAsync<T1, T2>(this IAsyncEnumerable<T1> objEnumerable, [NotNull] Func<T1, IAsyncEnumerable<T2>> funcSelector, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objEnumerable is IEnumerableWithAsync<T1> objEnumerableCast)
            {
                IEnumerator<T1> objEnumerator = await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    while (objEnumerator.MoveNext())
                    {
                        token.ThrowIfCancellationRequested();
                        await foreach (T2 objReturn in funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false))
                        {
                            if (objReturn != null)
                                yield return objReturn;
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
            else
            {
                await foreach (T1 objItem in objEnumerable.ConfigureAwait(false))
                {
                    token.ThrowIfCancellationRequested();
                    await foreach (T2 objReturn in funcSelector.Invoke(objItem).ConfigureAwait(false))
                    {
                        if (objReturn != null)
                            yield return objReturn;
                    }
                }
            }
        }

        /// <summary>
        /// Perform some function to select all elements from an async-capable enumerable or collection and pass it on.
        /// </summary>
        /// <param name="objEnumerable">Enumerable from which to select items to pass on.</param>
        /// <param name="funcSelector">Function to select what gets passed on to the output.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        internal static async IAsyncEnumerable<T2> SelectAsync<T1, T2>(this IAsyncEnumerable<T1> objEnumerable, [NotNull] Func<T1, IEnumerable<T2>> funcSelector, [EnumeratorCancellation] CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objEnumerable is IEnumerableWithAsync<T1> objEnumerableCast)
            {
                IEnumerator<T1> objEnumerator = await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    while (objEnumerator.MoveNext())
                    {
                        token.ThrowIfCancellationRequested();
                        foreach (T2 objReturn in funcSelector.Invoke(objEnumerator.Current))
                        {
                            if (objReturn != null)
                                yield return objReturn;
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
            else
            {
                await foreach (T1 objItem in objEnumerable.ConfigureAwait(false))
                {
                    token.ThrowIfCancellationRequested();
                    foreach (T2 objReturn in funcSelector.Invoke(objItem))
                    {
                        if (objReturn != null)
                            yield return objReturn;
                    }
                }
            }
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.Where{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        internal static async IAsyncEnumerable<T> DeepWhereAsync<T, T2>([ItemNotNull] this IAsyncEnumerable<T> objParentList, Func<T, T2> funcGetChildrenMethod, Func<T, bool> predicate, [EnumeratorCancellation] CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            await foreach (T objItem in objParentList.SelectAsync(Inner, token).ConfigureAwait(false))
            {
                yield return objItem;
            }
            async IAsyncEnumerable<T> Inner(T objLoopChild)
            {
                if (predicate(objLoopChild))
                    yield return objLoopChild;
                token.ThrowIfCancellationRequested();
                T2 lstChildren = funcGetChildrenMethod(objLoopChild);
                if (lstChildren is IEnumerableWithAsync<T> lstChildrenCast)
                {
                    await foreach (T objItem in lstChildrenCast.DeepWhereAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false))
                        yield return objItem;
                }
                else if (lstChildren is IAsyncEnumerable<T> lstChildrenCast2)
                {
                    await foreach (T objItem in lstChildrenCast2.DeepWhereAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false))
                        yield return objItem;
                }
                else
                {
                    foreach (T objItem in lstChildren.DeepWhere(funcGetChildrenMethod, predicate))
                    {
                        token.ThrowIfCancellationRequested();
                        yield return objItem;
                    }
                }
            }
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.Where{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        internal static async IAsyncEnumerable<T> DeepWhereAsync<T, T2>([ItemNotNull] this IAsyncEnumerable<T> objParentList, Func<T, Task<T2>> funcGetChildrenMethod, Func<T, bool> predicate, [EnumeratorCancellation] CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            await foreach (T objItem in objParentList.SelectAsync(Inner, token).ConfigureAwait(false))
            {
                yield return objItem;
            }
            async IAsyncEnumerable<T> Inner(T objLoopChild)
            {
                if (predicate(objLoopChild))
                    yield return objLoopChild;
                token.ThrowIfCancellationRequested();
                T2 lstChildren = await funcGetChildrenMethod(objLoopChild).ConfigureAwait(false);
                if (lstChildren is IEnumerableWithAsync<T> lstChildrenCast)
                {
                    await foreach (T objItem in lstChildrenCast.DeepWhereAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false))
                        yield return objItem;
                }
                else if (lstChildren is IAsyncEnumerable<T> lstChildrenCast2)
                {
                    await foreach (T objItem in lstChildrenCast2.DeepWhereAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false))
                        yield return objItem;
                }
                else
                {
                    await foreach (T objItem in lstChildren.DeepWhereAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false))
                        yield return objItem;
                }
            }
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.Where{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        internal static async IAsyncEnumerable<T> DeepWhereAsync<T, T2>([ItemNotNull] this IAsyncEnumerable<T> objParentList, Func<T, T2> funcGetChildrenMethod, Func<T, Task<bool>> predicate, [EnumeratorCancellation] CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            await foreach (T objItem in objParentList.SelectAsync(Inner, token).ConfigureAwait(false))
            {
                yield return objItem;
            }
            async IAsyncEnumerable<T> Inner(T objLoopChild)
            {
                if (await predicate(objLoopChild).ConfigureAwait(false))
                    yield return objLoopChild;
                token.ThrowIfCancellationRequested();
                T2 lstChildren = funcGetChildrenMethod(objLoopChild);
                if (lstChildren is IEnumerableWithAsync<T> lstChildrenCast)
                {
                    await foreach (T objItem in lstChildrenCast.DeepWhereAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false))
                        yield return objItem;
                }
                else if (lstChildren is IAsyncEnumerable<T> lstChildrenCast2)
                {
                    await foreach (T objItem in lstChildrenCast2.DeepWhereAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false))
                        yield return objItem;
                }
                else
                {
                    await foreach (T objItem in lstChildren.DeepWhereAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false))
                        yield return objItem;
                }
            }
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.Where{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        internal static async IAsyncEnumerable<T> DeepWhereAsync<T, T2>([ItemNotNull] this IAsyncEnumerable<T> objParentList, Func<T, Task<T2>> funcGetChildrenMethod, Func<T, Task<bool>> predicate, [EnumeratorCancellation] CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            await foreach (T objItem in objParentList.SelectAsync(Inner, token).ConfigureAwait(false))
            {
                yield return objItem;
            }
            async IAsyncEnumerable<T> Inner(T objLoopChild)
            {
                if (await predicate(objLoopChild).ConfigureAwait(false))
                    yield return objLoopChild;
                token.ThrowIfCancellationRequested();
                T2 lstChildren = await funcGetChildrenMethod(objLoopChild).ConfigureAwait(false);
                if (lstChildren is IEnumerableWithAsync<T> lstChildrenCast)
                {
                    await foreach (T objItem in lstChildrenCast.DeepWhereAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false))
                        yield return objItem;
                }
                else if (lstChildren is IAsyncEnumerable<T> lstChildrenCast2)
                {
                    await foreach (T objItem in lstChildrenCast2.DeepWhereAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false))
                        yield return objItem;
                }
                else
                {
                    await foreach (T objItem in lstChildren.DeepWhereAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false))
                        yield return objItem;
                }
            }
        }
    }
}
