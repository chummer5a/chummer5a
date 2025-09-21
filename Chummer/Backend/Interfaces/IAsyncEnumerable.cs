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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chummer.Annotations;

namespace Chummer
{
    public interface IAsyncEnumerable<T> : IEnumerable<T>
    {
        Task<IEnumerator<T>> GetEnumeratorAsync(CancellationToken token = default);
    }

    public static class AsyncEnumerableExtensions
    {
        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> objEnumerable, CancellationToken token = default)
        {
            List<T> lstReturn;
            switch (objEnumerable)
            {
                case IAsyncReadOnlyCollection<T> lstAsyncReadOnlyEnumerable:
                    lstReturn = new List<T>(await lstAsyncReadOnlyEnumerable.GetCountAsync(token).ConfigureAwait(false));
                    break;

                case ICollection<T> lstEnumerable:
                    lstReturn = new List<T>(lstEnumerable.Count);
                    break;

                case IReadOnlyCollection<T> lstReadOnlyEnumerable:
                    lstReturn = new List<T>(lstReadOnlyEnumerable.Count);
                    break;

                default:
                    lstReturn = new List<T>();
                    break;
            }

            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    lstReturn.Add(objEnumerator.Current);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return lstReturn;
        }

        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, CancellationToken token = default)
        {
            List<T> lstReturn;
            switch (objEnumerable)
            {
                case IAsyncReadOnlyCollection<T> lstAsyncReadOnlyEnumerable:
                    lstReturn = new List<T>(await lstAsyncReadOnlyEnumerable.GetCountAsync(token).ConfigureAwait(false));
                    break;

                case ICollection<T> lstEnumerable:
                    lstReturn = new List<T>(lstEnumerable.Count);
                    break;

                case IReadOnlyCollection<T> lstReadOnlyEnumerable:
                    lstReturn = new List<T>(lstReadOnlyEnumerable.Count);
                    break;

                default:
                    lstReturn = new List<T>();
                    break;
            }
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate.Invoke(objEnumerator.Current))
                        lstReturn.Add(objEnumerator.Current);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return lstReturn;
        }

        public static async Task<List<T>> ToListAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            List<T> lstReturn;
            switch (objEnumerable)
            {
                case IAsyncReadOnlyCollection<T> lstAsyncReadOnlyEnumerable:
                    lstReturn = new List<T>(await lstAsyncReadOnlyEnumerable.GetCountAsync(token).ConfigureAwait(false));
                    break;

                case ICollection<T> lstEnumerable:
                    lstReturn = new List<T>(lstEnumerable.Count);
                    break;

                case IReadOnlyCollection<T> lstReadOnlyEnumerable:
                    lstReturn = new List<T>(lstReadOnlyEnumerable.Count);
                    break;

                default:
                    lstReturn = new List<T>();
                    break;
            }

            IEnumerator<T> objEnumerator = objEnumerable is IAsyncEnumerable<T> objEnumerableCast
                ? await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objEnumerable.GetEnumerator();
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate.Invoke(objEnumerator.Current).ConfigureAwait(false))
                        lstReturn.Add(objEnumerator.Current);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return lstReturn;
        }

        public static async Task<List<T>> ToListAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await ToListAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, token).ConfigureAwait(false);
        }

        public static async Task<List<T>> ToListAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await ToListAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Async version of <see cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, int)"/> that also supports <see cref="IAsyncReadOnlyList{T}">.
        /// </summary>
        public static async Task<T> ElementAtBetterAsync<T>(this IAsyncEnumerable<T> source, int index, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (source is IAsyncReadOnlyList<T> list1)
                return await list1.GetValueAtAsync(index, token).ConfigureAwait(false);
            // Just in case we have classes that inherit from IList but not from IReadOnlyList
            if (source is IAsyncList<T> list2)
                return await list2.GetValueAtAsync(index, token).ConfigureAwait(false);
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            IEnumerator<T> objEnumerator = await source.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    if (index-- == 0)
                        return objEnumerator.Current;
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        /// <summary>
        /// Async version of <see cref="Enumerable.ElementAtOrDefault{TSource}(IEnumerable{TSource}, int)"/> that also supports <see cref="IAsyncReadOnlyList{T}">.
        /// </summary>
        public static async Task<T> ElementAtOrDefaultBetterAsync<T>(this IAsyncEnumerable<T> source, int index, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (index >= 0)
            {
                if (source is IAsyncReadOnlyList<T> list1)
                {
                    if (index < await list1.GetCountAsync(token).ConfigureAwait(false))
                    {
                        return await list1.GetValueAtAsync(index, token).ConfigureAwait(false);
                    }
                }
                // Just in case we have classes that inherit from IList but not from IReadOnlyList
                else if (source is IAsyncList<T> list2)
                {
                    if (index < await list2.GetCountAsync(token).ConfigureAwait(false))
                    {
                        return await list2.GetValueAtAsync(index, token).ConfigureAwait(false);
                    }
                }
                else
                {
                    IEnumerator<T> objEnumerator = await source.GetEnumeratorAsync(token).ConfigureAwait(false);
                    try
                    {
                        while (objEnumerator.MoveNext())
                        {
                            if (index-- == 0)
                                return objEnumerator.Current;
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
            }
            return default;
        }

        /// <summary>
        /// Get a HashCode representing the contents of an enumerable (instead of just of the pointer to the location where the enumerable would start)
        /// </summary>
        /// <typeparam name="T">The type for which <see cref="object.GetHashCode"/> will be called</typeparam>
        /// <param name="lstItems">The collection containing the contents</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>A HashCode that is generated based on the contents of <paramref name="lstItems"/></returns>
        public static async Task<int> GetEnsembleHashCodeAsync<T>(this IAsyncEnumerable<T> lstItems, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstItems == null)
                return 0;
            unchecked
            {
                // uint to prevent overflows
                uint result = 19u;
                IEnumerator<T> objEnumerator = await lstItems.GetEnumeratorAsync(token).ConfigureAwait(false);
                try
                {
                    while (objEnumerator.MoveNext())
                    {
                        token.ThrowIfCancellationRequested();
                        result = result * 31u + (uint)objEnumerator.Current.GetHashCode();
                    }
                }
                finally
                {
                    if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                        await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                    else
                        objEnumerator.Dispose();
                }

                return (int)result;
            }
        }

        /// <summary>
        /// Get a HashCode representing the contents of an enumerable (instead of just of the pointer to the location where the enumerable would start) in a way where the order of the items is irrelevant
        /// NOTE: GetEnsembleHashCode and GetOrderInvariantEnsembleHashCode will almost never be the same for the same collection!
        /// </summary>
        /// <typeparam name="T">The type for which <see cref="object.GetHashCode"/> will be called</typeparam>
        /// <param name="lstItems">The collection containing the contents</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>A HashCode that is generated based on the contents of <paramref name="lstItems"/></returns>
        public static async Task<int> GetOrderInvariantEnsembleHashCodeAsync<T>(this IAsyncEnumerable<T> lstItems, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstItems == null)
                return 0;
            // uint to prevent overflows
            unchecked
            {
                uint result = 0;
                IEnumerator<T> objEnumerator = await lstItems.GetEnumeratorAsync(token).ConfigureAwait(false);
                try
                {
                    while (objEnumerator.MoveNext())
                    {
                        token.ThrowIfCancellationRequested();
                        result += (uint)objEnumerator.Current.GetHashCode();
                    }
                }
                finally
                {
                    if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                        await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                    else
                        objEnumerator.Dispose();
                }
                return (int)(19u + result * 31u);
            }
        }

        /// <summary>
        /// Get a HashCode representing the contents of a collection in a way where the order of the items is irrelevant
        /// This is a parallelized version of GetOrderInvariantEnsembleHashCode meant to be used for large collections
        /// NOTE: GetEnsembleHashCode and GetOrderInvariantEnsembleHashCode will almost never be the same for the same collection!
        /// </summary>
        /// <typeparam name="T">The type for which <see cref="object.GetHashCode"/> will be called</typeparam>
        /// <param name="lstItems">The collection containing the contents</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>A HashCode that is generated based on the contents of <paramref name="lstItems"/></returns>
        public static async Task<int> GetOrderInvariantEnsembleHashCodeParallelAsync<T>(this IAsyncEnumerable<T> lstItems, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstItems == null)
                return 0;
            List<Task<int>> lstTasks = lstItems is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            // uint to prevent overflows
            unchecked
            {
                uint result = 0;
                IEnumerator<T> objEnumerator = await lstItems.GetEnumeratorAsync(token).ConfigureAwait(false);
                try
                {
                    bool blnMoveNext = objEnumerator.MoveNext();
                    while (blnMoveNext)
                    {
                        for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                        {
                            token.ThrowIfCancellationRequested();
                            T objCurrent = objEnumerator.Current;
                            lstTasks.Add(Task.Run(() => objCurrent.GetHashCode(), token));
                            blnMoveNext = objEnumerator.MoveNext();
                        }

                        if (blnMoveNext)
                        {
                            token.ThrowIfCancellationRequested();
                            await Task.WhenAll(lstTasks).ConfigureAwait(false);
                            token.ThrowIfCancellationRequested();
                            foreach (Task<int> tskLoop in lstTasks)
                                result += (uint)await tskLoop.ConfigureAwait(false);
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
                    result += (uint)await tskLoop.ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                return (int)(19u + result * 31u);
            }
        }

        public static bool Any<T>(this IAsyncEnumerable<T> objEnumerable, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                token.ThrowIfCancellationRequested();
                if (objEnumerator.MoveNext())
                {
                    return true;
                }
            }
            return false;
        }

        public static bool Any<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                token.ThrowIfCancellationRequested();
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate.Invoke(objEnumerator.Current))
                        return true;
                }
            }
            return false;
        }

        public static async Task<bool> AnyAsync<T>(this IAsyncEnumerable<T> objEnumerable, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                if (objEnumerator.MoveNext())
                {
                    return true;
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return false;
        }

        public static async Task<bool> AnyAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate.Invoke(objEnumerator.Current))
                        return true;
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return false;
        }

        public static async Task<bool> AnyAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IEnumerator<T> objEnumerator = objEnumerable is IAsyncEnumerable<T> objEnumerableCast
                ? await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objEnumerable.GetEnumerator();
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate.Invoke(objEnumerator.Current).ConfigureAwait(false))
                        return true;
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return false;
        }

        public static async Task<bool> AnyAsync<T, T2>(this Task<T2> tskEnumerable, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await AnyAsync(await tskEnumerable.ConfigureAwait(false), token).ConfigureAwait(false);
        }

        public static async Task<bool> AnyAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await AnyAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, token).ConfigureAwait(false);
        }

        public static async Task<bool> AnyAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            return await AnyAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, token).ConfigureAwait(false);
        }

        public static bool All<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                token.ThrowIfCancellationRequested();
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (!funcPredicate.Invoke(objEnumerator.Current))
                        return false;
                }
            }
            return true;
        }

        public static async Task<bool> AllAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (!funcPredicate.Invoke(objEnumerator.Current))
                        return false;
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return true;
        }

        public static async Task<bool> AllAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IEnumerator<T> objEnumerator = objEnumerable is IAsyncEnumerable<T> objEnumerableCast
                ? await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objEnumerable.GetEnumerator();
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (!await funcPredicate.Invoke(objEnumerator.Current).ConfigureAwait(false))
                        return false;
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return true;
        }

        public static async Task<bool> AllAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await AllAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, token).ConfigureAwait(false);
        }

        public static async Task<bool> AllAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            return await AllAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, token).ConfigureAwait(false);
        }

        public static int Count<T>(this IAsyncEnumerable<T> objEnumerable, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = 0;
            using (IEnumerator<T> objEnumerator = objEnumerable.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    ++intReturn;
                }
            }
            return intReturn;
        }

        public static int Count<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = 0;
            foreach (T objItem in objEnumerable)
            {
                token.ThrowIfCancellationRequested();
                if (funcPredicate.Invoke(objItem))
                    ++intReturn;
            }
            return intReturn;
        }

        public static async Task<int> CountAsync<T>(this IAsyncEnumerable<T> objEnumerable, CancellationToken token = default)
        {
#if DEBUG
            if (objEnumerable is IAsyncCollection<T>)
            {
                // Are you sure you want this instead of GetCountAsync<T>?
                Utils.BreakIfDebug();
            }
#endif
            token.ThrowIfCancellationRequested();
            int intReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    ++intReturn;
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

        public static async Task<int> CountAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate.Invoke(objEnumerator.Current))
                        ++intReturn;
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

        public static async Task<int> CountAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = 0;
            IEnumerator<T> objEnumerator = objEnumerable is IAsyncEnumerable<T> objEnumerableCast
                ? await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objEnumerable.GetEnumerator();
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate.Invoke(objEnumerator.Current).ConfigureAwait(false))
                        ++intReturn;
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

        public static async Task<int> CountAsync<T, T2>(this Task<T2> tskEnumerable, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await CountAsync(await tskEnumerable.ConfigureAwait(false), token).ConfigureAwait(false);
        }

        public static async Task<int> CountAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await CountAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, token).ConfigureAwait(false);
        }

        public static async Task<int> CountAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            return await CountAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, token).ConfigureAwait(false);
        }

        public static async Task<int> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<int> SumAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = 0;
            IEnumerator<T> objEnumerator = objEnumerable is IAsyncEnumerable<T> objEnumerableCast
                ? await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objEnumerable.GetEnumerator();
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

        public static async Task<int> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<long> SumAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = 0;
            IEnumerator<T> objEnumerator = objEnumerable is IAsyncEnumerable<T> objEnumerableCast
                ? await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objEnumerable.GetEnumerator();
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

        public static async Task<long> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            float fltReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<float> SumAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            float fltReturn = 0;
            IEnumerator<T> objEnumerator = objEnumerable is IAsyncEnumerable<T> objEnumerableCast
                ? await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objEnumerable.GetEnumerator();
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

        public static async Task<float> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<double> SumAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn = 0;
            IEnumerator<T> objEnumerator = objEnumerable is IAsyncEnumerable<T> objEnumerableCast
                ? await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objEnumerable.GetEnumerator();
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

        public static async Task<double> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<decimal> SumAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = 0;
            IEnumerator<T> objEnumerator = objEnumerable is IAsyncEnumerable<T> objEnumerableCast
                ? await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objEnumerable.GetEnumerator();
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

        public static async Task<decimal> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<int> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<int> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<long> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<long> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<float> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<float> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<double> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<double> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<decimal> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<decimal> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<int> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<int> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<long> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<long> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            float fltReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<float> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            float fltReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<float> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<double> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<double> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<decimal> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<decimal> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<int> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<int> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<long> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<long> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<float> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<float> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<double> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<double> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<decimal> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<decimal> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<int> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<int> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<long> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<long> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            float fltReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<float> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            float fltReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<float> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<double> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<double> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<decimal> SumAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<decimal> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> SumAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<int> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<int> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<long> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<long> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<float> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<float> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<double> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<double> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<decimal> SumParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = 0;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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

        public static async Task<decimal> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> SumParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await SumParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<T> AggregateAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, T, T> funcAggregator, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            T objReturn;
            IEnumerator<T> objEnumerator = objEnumerable is IAsyncEnumerable<T> objEnumerableCast
                ? await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objEnumerable.GetEnumerator();
            try
            {
                if (!objEnumerator.MoveNext())
                {
                    throw new ArgumentException("Enumerable has no elements", nameof(objEnumerable));
                }

                objReturn = objEnumerator.Current;
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    objReturn = funcAggregator(objReturn, objEnumerator.Current);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return objReturn;
        }

        public static async Task<T> AggregateAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, T, Task<T>> funcAggregator, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            T objReturn;
            IEnumerator<T> objEnumerator = objEnumerable is IAsyncEnumerable<T> objEnumerableCast
                ? await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objEnumerable.GetEnumerator();
            try
            {
                if (!objEnumerator.MoveNext())
                {
                    throw new ArgumentException("Enumerable has no elements", nameof(objEnumerable));
                }

                objReturn = objEnumerator.Current;
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    objReturn = await funcAggregator(objReturn, objEnumerator.Current).ConfigureAwait(false);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return objReturn;
        }

        public static async Task<T> AggregateAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, T, T> funcAggregator, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await AggregateAsync(await tskEnumerable.ConfigureAwait(false), funcAggregator, token).ConfigureAwait(false);
        }

        public static async Task<T> AggregateAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, T, Task<T>> funcAggregator, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            return await AggregateAsync(await tskEnumerable.ConfigureAwait(false), funcAggregator, token).ConfigureAwait(false);
        }

        public static async Task<TAccumulate> AggregateAsync<T, TAccumulate>(this IEnumerable<T> objEnumerable, TAccumulate objSeed, [NotNull] Func<TAccumulate, T, TAccumulate> funcAggregator, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            TAccumulate objReturn = objSeed;
            IEnumerator<T> objEnumerator = objEnumerable is IAsyncEnumerable<T> objEnumerableCast
                ? await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objEnumerable.GetEnumerator();
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    objReturn = funcAggregator(objReturn, objEnumerator.Current);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return objReturn;
        }

        public static async Task<TAccumulate> AggregateAsync<T, TAccumulate>(this IEnumerable<T> objEnumerable, TAccumulate objSeed, [NotNull] Func<TAccumulate, T, Task<TAccumulate>> funcAggregator, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            TAccumulate objReturn = objSeed;
            IEnumerator<T> objEnumerator = objEnumerable is IAsyncEnumerable<T> objEnumerableCast
                ? await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objEnumerable.GetEnumerator();
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    objReturn = await funcAggregator(objReturn, objEnumerator.Current).ConfigureAwait(false);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return objReturn;
        }

        public static async Task<TAccumulate> AggregateAsync<T, TAccumulate, T2>(this Task<T2> tskEnumerable, TAccumulate objSeed, [NotNull] Func<TAccumulate, T, TAccumulate> funcAggregator, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await AggregateAsync(await tskEnumerable.ConfigureAwait(false), objSeed, funcAggregator, token).ConfigureAwait(false);
        }

        public static async Task<TAccumulate> AggregateAsync<T, TAccumulate, T2>(this Task<T2> tskEnumerable, TAccumulate objSeed, [NotNull] Func<TAccumulate, T, Task<TAccumulate>> funcAggregator, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            return await AggregateAsync(await tskEnumerable.ConfigureAwait(false), objSeed, funcAggregator, token).ConfigureAwait(false);
        }

        public static async Task<int> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = int.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    intReturn = Math.Max(intReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<int> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = int.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    intReturn = Math.Max(intReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<int> MaxAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            int intReturn = int.MinValue;
            foreach (T objItem in objEnumerable)
            {
                token.ThrowIfCancellationRequested();
                intReturn = Math.Max(intReturn, await funcSelector.Invoke(objItem).ConfigureAwait(false));
            }
            return intReturn;
        }

        public static async Task<int> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = long.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    lngReturn = Math.Max(lngReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<long> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = long.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    lngReturn = Math.Max(lngReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<long> MaxAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            long lngReturn = long.MinValue;
            foreach (T objItem in objEnumerable)
            {
                token.ThrowIfCancellationRequested();
                lngReturn = Math.Max(lngReturn, await funcSelector.Invoke(objItem).ConfigureAwait(false));
            }
            return lngReturn;
        }

        public static async Task<long> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            float fltReturn = float.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    fltReturn = Math.Max(fltReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<float> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            float fltReturn = float.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    fltReturn = Math.Max(fltReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<float> MaxAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            float fltReturn = float.MinValue;
            foreach (T objItem in objEnumerable)
            {
                token.ThrowIfCancellationRequested();
                fltReturn = Math.Max(fltReturn, await funcSelector.Invoke(objItem).ConfigureAwait(false));
            }
            return fltReturn;
        }

        public static async Task<float> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn = double.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    dblReturn = Math.Max(dblReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<double> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn = double.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    dblReturn = Math.Max(dblReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<double> MaxAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            double dblReturn = double.MinValue;
            foreach (T objItem in objEnumerable)
            {
                token.ThrowIfCancellationRequested();
                dblReturn = Math.Max(dblReturn, await funcSelector.Invoke(objItem).ConfigureAwait(false));
            }
            return dblReturn;
        }

        public static async Task<double> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = decimal.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    decReturn = Math.Max(decReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<decimal> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = decimal.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    decReturn = Math.Max(decReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<decimal> MaxAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            decimal decReturn = decimal.MinValue;
            foreach (T objItem in objEnumerable)
            {
                token.ThrowIfCancellationRequested();
                decReturn = Math.Max(decReturn, await funcSelector.Invoke(objItem).ConfigureAwait(false));
            }
            return decReturn;
        }

        public static async Task<decimal> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = int.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            intReturn = Math.Max(intReturn, await tskLoop.ConfigureAwait(false));
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
                intReturn = Math.Max(intReturn, await tskLoop.ConfigureAwait(false));
            return intReturn;
        }

        public static async Task<int> MaxParallelAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = int.MinValue;
            IAsyncEnumerable<T> objEnumerableCast = objEnumerable as IAsyncEnumerable<T>;
            List<Task<int>> lstTasks;
            if (objEnumerableCast != null)
            {
                lstTasks = objEnumerableCast is IAsyncReadOnlyCollection<T> objTemp
                    ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize,
                        await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                    : new List<Task<int>>(Utils.MaxParallelBatchSize);
            }
            else switch (objEnumerable)
            {
                case ICollection<T> objTemp:
                    lstTasks = new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, objTemp.Count));
                    break;
                case IReadOnlyCollection<T> objTemp2:
                    lstTasks = new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, objTemp2.Count));
                    break;
                default:
                    lstTasks = new List<Task<int>>(Utils.MaxParallelBatchSize);
                    break;
            }
            IEnumerator<T> objEnumerator = objEnumerableCast != null
                ? await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objEnumerable.GetEnumerator();
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
                            intReturn = Math.Max(intReturn, await tskLoop.ConfigureAwait(false));
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
                intReturn = Math.Max(intReturn, await tskLoop.ConfigureAwait(false));
            return intReturn;
        }

        public static async Task<int> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxParallelAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxParallelAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxParallelAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = long.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            lngReturn = Math.Max(lngReturn, await tskLoop.ConfigureAwait(false));
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
                lngReturn = Math.Max(lngReturn, await tskLoop.ConfigureAwait(false));
            return lngReturn;
        }

        public static async Task<long> MaxParallelAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = long.MinValue;
            IAsyncEnumerable<T> objEnumerableCast = objEnumerable as IAsyncEnumerable<T>;
            List<Task<long>> lstTasks;
            if (objEnumerableCast != null)
            {
                lstTasks = objEnumerableCast is IAsyncReadOnlyCollection<T> objTemp
                    ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize,
                        await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                    : new List<Task<long>>(Utils.MaxParallelBatchSize);
            }
            else switch (objEnumerable)
            {
                case ICollection<T> objTemp:
                    lstTasks = new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, objTemp.Count));
                    break;
                case IReadOnlyCollection<T> objTemp2:
                    lstTasks = new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, objTemp2.Count));
                    break;
                default:
                    lstTasks = new List<Task<long>>(Utils.MaxParallelBatchSize);
                    break;
            }
            IEnumerator<T> objEnumerator = objEnumerableCast != null
                ? await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objEnumerable.GetEnumerator();
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
                            lngReturn = Math.Max(lngReturn, await tskLoop.ConfigureAwait(false));
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
                lngReturn = Math.Max(lngReturn, await tskLoop.ConfigureAwait(false));
            return lngReturn;
        }

        public static async Task<long> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxParallelAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxParallelAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxParallelAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = float.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            fltReturn = Math.Max(fltReturn, await tskLoop.ConfigureAwait(false));
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
                fltReturn = Math.Max(fltReturn, await tskLoop.ConfigureAwait(false));
            return fltReturn;
        }

        public static async Task<float> MaxParallelAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            float fltReturn = float.MinValue;
            IAsyncEnumerable<T> objEnumerableCast = objEnumerable as IAsyncEnumerable<T>;
            List<Task<float>> lstTasks;
            if (objEnumerableCast != null)
            {
                lstTasks = objEnumerableCast is IAsyncReadOnlyCollection<T> objTemp
                    ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize,
                        await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                    : new List<Task<float>>(Utils.MaxParallelBatchSize);
            }
            else switch (objEnumerable)
            {
                case ICollection<T> objTemp:
                    lstTasks = new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, objTemp.Count));
                    break;
                case IReadOnlyCollection<T> objTemp2:
                    lstTasks = new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, objTemp2.Count));
                    break;
                default:
                    lstTasks = new List<Task<float>>(Utils.MaxParallelBatchSize);
                    break;
            }
            IEnumerator<T> objEnumerator = objEnumerableCast != null
                ? await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objEnumerable.GetEnumerator();
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
                            fltReturn = Math.Max(fltReturn, await tskLoop.ConfigureAwait(false));
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
                fltReturn = Math.Max(fltReturn, await tskLoop.ConfigureAwait(false));
            return fltReturn;
        }

        public static async Task<float> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxParallelAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxParallelAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxParallelAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = double.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            dblReturn = Math.Max(dblReturn, await tskLoop.ConfigureAwait(false));
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
                dblReturn = Math.Max(dblReturn, await tskLoop.ConfigureAwait(false));
            return dblReturn;
        }

        public static async Task<double> MaxParallelAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn = double.MinValue;
            IAsyncEnumerable<T> objEnumerableCast = objEnumerable as IAsyncEnumerable<T>;
            List<Task<double>> lstTasks;
            if (objEnumerableCast != null)
            {
                lstTasks = objEnumerableCast is IAsyncReadOnlyCollection<T> objTemp
                    ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize,
                        await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                    : new List<Task<double>>(Utils.MaxParallelBatchSize);
            }
            else switch (objEnumerable)
            {
                case ICollection<T> objTemp:
                    lstTasks = new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, objTemp.Count));
                    break;
                case IReadOnlyCollection<T> objTemp2:
                    lstTasks = new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, objTemp2.Count));
                    break;
                default:
                    lstTasks = new List<Task<double>>(Utils.MaxParallelBatchSize);
                    break;
            }
            IEnumerator<T> objEnumerator = objEnumerableCast != null
                ? await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objEnumerable.GetEnumerator();
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
                            dblReturn = Math.Max(dblReturn, await tskLoop.ConfigureAwait(false));
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
                dblReturn = Math.Max(dblReturn, await tskLoop.ConfigureAwait(false));
            return dblReturn;
        }

        public static async Task<double> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxParallelAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxParallelAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxParallelAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = decimal.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            decReturn = Math.Max(decReturn, await tskLoop.ConfigureAwait(false));
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
                decReturn = Math.Max(decReturn, await tskLoop.ConfigureAwait(false));
            return decReturn;
        }

        public static async Task<decimal> MaxParallelAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = decimal.MinValue;
            IAsyncEnumerable<T> objEnumerableCast = objEnumerable as IAsyncEnumerable<T>;
            List<Task<decimal>> lstTasks;
            if (objEnumerableCast != null)
            {
                lstTasks = objEnumerableCast is IAsyncReadOnlyCollection<T> objTemp
                    ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize,
                        await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                    : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            }
            else switch (objEnumerable)
            {
                case ICollection<T> objTemp:
                    lstTasks = new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, objTemp.Count));
                    break;
                case IReadOnlyCollection<T> objTemp2:
                    lstTasks = new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, objTemp2.Count));
                    break;
                default:
                    lstTasks = new List<Task<decimal>>(Utils.MaxParallelBatchSize);
                    break;
            }
            IEnumerator<T> objEnumerator = objEnumerableCast != null
                ? await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objEnumerable.GetEnumerator();
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
                            decReturn = Math.Max(decReturn, await tskLoop.ConfigureAwait(false));
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
                decReturn = Math.Max(decReturn, await tskLoop.ConfigureAwait(false));
            return decReturn;
        }

        public static async Task<decimal> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxParallelAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxParallelAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxParallelAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = int.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        intReturn = Math.Max(intReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<int> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = int.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        intReturn = Math.Max(intReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<int> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = long.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        lngReturn = Math.Max(lngReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<long> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = long.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        lngReturn = Math.Max(lngReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<long> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            float fltReturn = float.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        fltReturn = Math.Max(fltReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<float> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            float fltReturn = float.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        fltReturn = Math.Max(fltReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<float> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn = double.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        dblReturn = Math.Max(dblReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<double> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn = double.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        dblReturn = Math.Max(dblReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<double> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = decimal.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        decReturn = Math.Max(decReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<decimal> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = decimal.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        decReturn = Math.Max(decReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<decimal> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = int.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            intReturn = Math.Max(intReturn, await tskLoop.ConfigureAwait(false));
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
                intReturn = Math.Max(intReturn, await tskLoop.ConfigureAwait(false));
            return intReturn;
        }

        public static async Task<int> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = int.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            intReturn = Math.Max(intReturn, await tskLoop.ConfigureAwait(false));
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
                intReturn = Math.Max(intReturn, await tskLoop.ConfigureAwait(false));
            return intReturn;
        }

        public static async Task<int> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxParallelAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxParallelAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = long.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            lngReturn = Math.Max(lngReturn, await tskLoop.ConfigureAwait(false));
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
                lngReturn = Math.Max(lngReturn, await tskLoop.ConfigureAwait(false));
            return lngReturn;
        }

        public static async Task<long> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = long.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            lngReturn = Math.Max(lngReturn, await tskLoop.ConfigureAwait(false));
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
                lngReturn = Math.Max(lngReturn, await tskLoop.ConfigureAwait(false));
            return lngReturn;
        }

        public static async Task<long> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxParallelAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxParallelAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = float.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            fltReturn = Math.Max(fltReturn, await tskLoop.ConfigureAwait(false));
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
                fltReturn = Math.Max(fltReturn, await tskLoop.ConfigureAwait(false));
            return fltReturn;
        }

        public static async Task<float> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = float.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            fltReturn = Math.Max(fltReturn, await tskLoop.ConfigureAwait(false));
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
                fltReturn = Math.Max(fltReturn, await tskLoop.ConfigureAwait(false));
            return fltReturn;
        }

        public static async Task<float> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxParallelAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxParallelAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = double.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            dblReturn = Math.Max(dblReturn, await tskLoop.ConfigureAwait(false));
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
                dblReturn = Math.Max(dblReturn, await tskLoop.ConfigureAwait(false));
            return dblReturn;
        }

        public static async Task<double> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = double.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            dblReturn = Math.Max(dblReturn, await tskLoop.ConfigureAwait(false));
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
                dblReturn = Math.Max(dblReturn, await tskLoop.ConfigureAwait(false));
            return dblReturn;
        }

        public static async Task<double> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxParallelAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxParallelAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = decimal.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            decReturn = Math.Max(decReturn, await tskLoop.ConfigureAwait(false));
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
                decReturn = Math.Max(decReturn, await tskLoop.ConfigureAwait(false));
            return decReturn;
        }

        public static async Task<decimal> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = decimal.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            decReturn = Math.Max(decReturn, await tskLoop.ConfigureAwait(false));
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
                decReturn = Math.Max(decReturn, await tskLoop.ConfigureAwait(false));
            return decReturn;
        }

        public static async Task<decimal> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxParallelAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxParallelAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = int.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        intReturn = Math.Max(intReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<int> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = int.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        intReturn = Math.Max(intReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<int> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = long.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        lngReturn = Math.Max(lngReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<long> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = long.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        lngReturn = Math.Max(lngReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<long> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            float fltReturn = float.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        fltReturn = Math.Max(fltReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<float> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            float fltReturn = float.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        fltReturn = Math.Max(fltReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<float> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn = double.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        dblReturn = Math.Max(dblReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<double> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn = double.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        dblReturn = Math.Max(dblReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<double> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = decimal.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        decReturn = Math.Max(decReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<decimal> MaxAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = decimal.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        decReturn = Math.Max(decReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<decimal> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> MaxAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = int.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            intReturn = Math.Max(intReturn, await tskLoop.ConfigureAwait(false));
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
                intReturn = Math.Max(intReturn, await tskLoop.ConfigureAwait(false));
            return intReturn;
        }

        public static async Task<int> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = int.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            intReturn = Math.Max(intReturn, await tskLoop.ConfigureAwait(false));
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
                intReturn = Math.Max(intReturn, await tskLoop.ConfigureAwait(false));
            return intReturn;
        }

        public static async Task<int> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxParallelAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxParallelAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = long.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            lngReturn = Math.Max(lngReturn, await tskLoop.ConfigureAwait(false));
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
                lngReturn = Math.Max(lngReturn, await tskLoop.ConfigureAwait(false));
            return lngReturn;
        }

        public static async Task<long> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = long.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            lngReturn = Math.Max(lngReturn, await tskLoop.ConfigureAwait(false));
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
                lngReturn = Math.Max(lngReturn, await tskLoop.ConfigureAwait(false));
            return lngReturn;
        }

        public static async Task<long> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxParallelAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxParallelAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = float.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            fltReturn = Math.Max(fltReturn, await tskLoop.ConfigureAwait(false));
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
                fltReturn = Math.Max(fltReturn, await tskLoop.ConfigureAwait(false));
            return fltReturn;
        }

        public static async Task<float> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = float.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            fltReturn = Math.Max(fltReturn, await tskLoop.ConfigureAwait(false));
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
                fltReturn = Math.Max(fltReturn, await tskLoop.ConfigureAwait(false));
            return fltReturn;
        }

        public static async Task<float> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxParallelAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxParallelAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = double.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            dblReturn = Math.Max(dblReturn, await tskLoop.ConfigureAwait(false));
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
                dblReturn = Math.Max(dblReturn, await tskLoop.ConfigureAwait(false));
            return dblReturn;
        }

        public static async Task<double> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = double.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            dblReturn = Math.Max(dblReturn, await tskLoop.ConfigureAwait(false));
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
                dblReturn = Math.Max(dblReturn, await tskLoop.ConfigureAwait(false));
            return dblReturn;
        }

        public static async Task<double> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxParallelAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxParallelAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = decimal.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            decReturn = Math.Max(decReturn, await tskLoop.ConfigureAwait(false));
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
                decReturn = Math.Max(decReturn, await tskLoop.ConfigureAwait(false));
            return decReturn;
        }

        public static async Task<decimal> MaxParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = decimal.MinValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            decReturn = Math.Max(decReturn, await tskLoop.ConfigureAwait(false));
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
                decReturn = Math.Max(decReturn, await tskLoop.ConfigureAwait(false));
            return decReturn;
        }

        public static async Task<decimal> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MaxParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> MaxParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await MaxParallelAsync(lstEnumerableCast, funcSelector, token).ConfigureAwait(false);
            return await MaxParallelAsync(lstEnumerable, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = int.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    intReturn = Math.Min(intReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<int> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = int.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    intReturn = Math.Min(intReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<int> MinAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            int intReturn = int.MaxValue;
            foreach (T objItem in objEnumerable)
            {
                token.ThrowIfCancellationRequested();
                intReturn = Math.Min(intReturn, await funcSelector.Invoke(objItem).ConfigureAwait(false));
            }
            return intReturn;
        }

        public static async Task<int> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> MinAsync<T>(this Task<IEnumerable<T>> tskEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = long.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    lngReturn = Math.Min(lngReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<long> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = long.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    lngReturn = Math.Min(lngReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<long> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            float fltReturn = float.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    fltReturn = Math.Min(fltReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<float> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            float fltReturn = float.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    fltReturn = Math.Min(fltReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<float> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn = double.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    dblReturn = Math.Min(dblReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<double> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn = double.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    dblReturn = Math.Min(dblReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<double> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = decimal.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    decReturn = Math.Min(decReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<decimal> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = decimal.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    decReturn = Math.Min(decReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<decimal> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = int.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            intReturn = Math.Min(intReturn, await tskLoop.ConfigureAwait(false));
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
                intReturn = Math.Min(intReturn, await tskLoop.ConfigureAwait(false));
            return intReturn;
        }

        public static async Task<int> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = int.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            intReturn = Math.Min(intReturn, await tskLoop.ConfigureAwait(false));
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
                intReturn = Math.Min(intReturn, await tskLoop.ConfigureAwait(false));
            return intReturn;
        }

        public static async Task<int> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, int> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = long.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            lngReturn = Math.Min(lngReturn, await tskLoop.ConfigureAwait(false));
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
                lngReturn = Math.Min(lngReturn, await tskLoop.ConfigureAwait(false));
            return lngReturn;
        }

        public static async Task<long> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = long.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            lngReturn = Math.Min(lngReturn, await tskLoop.ConfigureAwait(false));
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
                lngReturn = Math.Min(lngReturn, await tskLoop.ConfigureAwait(false));
            return lngReturn;
        }

        public static async Task<long> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, long> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = float.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            fltReturn = Math.Min(fltReturn, await tskLoop.ConfigureAwait(false));
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
                fltReturn = Math.Min(fltReturn, await tskLoop.ConfigureAwait(false));
            return fltReturn;
        }

        public static async Task<float> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = float.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            fltReturn = Math.Min(fltReturn, await tskLoop.ConfigureAwait(false));
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
                fltReturn = Math.Min(fltReturn, await tskLoop.ConfigureAwait(false));
            return fltReturn;
        }

        public static async Task<float> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, float> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = double.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            dblReturn = Math.Min(dblReturn, await tskLoop.ConfigureAwait(false));
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
                dblReturn = Math.Min(dblReturn, await tskLoop.ConfigureAwait(false));
            return dblReturn;
        }

        public static async Task<double> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = double.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            dblReturn = Math.Min(dblReturn, await tskLoop.ConfigureAwait(false));
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
                dblReturn = Math.Min(dblReturn, await tskLoop.ConfigureAwait(false));
            return dblReturn;
        }

        public static async Task<double> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, double> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = decimal.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            decReturn = Math.Min(decReturn, await tskLoop.ConfigureAwait(false));
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
                decReturn = Math.Min(decReturn, await tskLoop.ConfigureAwait(false));
            return decReturn;
        }

        public static async Task<decimal> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = decimal.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            decReturn = Math.Min(decReturn, await tskLoop.ConfigureAwait(false));
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
                decReturn = Math.Min(decReturn, await tskLoop.ConfigureAwait(false));
            return decReturn;
        }

        public static async Task<decimal> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = int.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        intReturn = Math.Min(intReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<int> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = int.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        intReturn = Math.Min(intReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<int> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = long.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        lngReturn = Math.Min(lngReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<long> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = long.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        lngReturn = Math.Min(lngReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<long> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            float fltReturn = float.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        fltReturn = Math.Min(fltReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<float> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            float fltReturn = float.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        fltReturn = Math.Min(fltReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<float> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn = double.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        dblReturn = Math.Min(dblReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<double> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn = double.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        dblReturn = Math.Min(dblReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<double> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = decimal.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        decReturn = Math.Min(decReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<decimal> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = decimal.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate(objEnumerator.Current))
                        decReturn = Math.Min(decReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<decimal> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = int.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            intReturn = Math.Min(intReturn, await tskLoop.ConfigureAwait(false));
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
                intReturn = Math.Min(intReturn, await tskLoop.ConfigureAwait(false));
            return intReturn;
        }

        public static async Task<int> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = int.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            intReturn = Math.Min(intReturn, await tskLoop.ConfigureAwait(false));
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
                intReturn = Math.Min(intReturn, await tskLoop.ConfigureAwait(false));
            return intReturn;
        }

        public static async Task<int> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = long.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            lngReturn = Math.Min(lngReturn, await tskLoop.ConfigureAwait(false));
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
                lngReturn = Math.Min(lngReturn, await tskLoop.ConfigureAwait(false));
            return lngReturn;
        }

        public static async Task<long> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = long.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            lngReturn = Math.Min(lngReturn, await tskLoop.ConfigureAwait(false));
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
                lngReturn = Math.Min(lngReturn, await tskLoop.ConfigureAwait(false));
            return lngReturn;
        }

        public static async Task<long> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = float.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            fltReturn = Math.Min(fltReturn, await tskLoop.ConfigureAwait(false));
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
                fltReturn = Math.Min(fltReturn, await tskLoop.ConfigureAwait(false));
            return fltReturn;
        }

        public static async Task<float> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = float.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            fltReturn = Math.Min(fltReturn, await tskLoop.ConfigureAwait(false));
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
                fltReturn = Math.Min(fltReturn, await tskLoop.ConfigureAwait(false));
            return fltReturn;
        }

        public static async Task<float> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = double.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            dblReturn = Math.Min(dblReturn, await tskLoop.ConfigureAwait(false));
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
                dblReturn = Math.Min(dblReturn, await tskLoop.ConfigureAwait(false));
            return dblReturn;
        }

        public static async Task<double> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = double.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            dblReturn = Math.Min(dblReturn, await tskLoop.ConfigureAwait(false));
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
                dblReturn = Math.Min(dblReturn, await tskLoop.ConfigureAwait(false));
            return dblReturn;
        }

        public static async Task<double> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = decimal.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            decReturn = Math.Min(decReturn, await tskLoop.ConfigureAwait(false));
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
                decReturn = Math.Min(decReturn, await tskLoop.ConfigureAwait(false));
            return decReturn;
        }

        public static async Task<decimal> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = decimal.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            decReturn = Math.Min(decReturn, await tskLoop.ConfigureAwait(false));
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
                decReturn = Math.Min(decReturn, await tskLoop.ConfigureAwait(false));
            return decReturn;
        }

        public static async Task<decimal> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = int.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        intReturn = Math.Min(intReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<int> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn = int.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        intReturn = Math.Min(intReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<int> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = long.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        lngReturn = Math.Min(lngReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<long> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = long.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        lngReturn = Math.Min(lngReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<long> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            float fltReturn = float.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        fltReturn = Math.Min(fltReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<float> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            float fltReturn = float.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        fltReturn = Math.Min(fltReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<float> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn = double.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        dblReturn = Math.Min(dblReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<double> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn = double.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        dblReturn = Math.Min(dblReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<double> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = decimal.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        decReturn = Math.Min(decReturn, funcSelector.Invoke(objEnumerator.Current));
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

        public static async Task<decimal> MinAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = decimal.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate(objEnumerator.Current).ConfigureAwait(false))
                        decReturn = Math.Min(decReturn, await funcSelector.Invoke(objEnumerator.Current).ConfigureAwait(false));
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

        public static async Task<decimal> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> MinAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = int.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            intReturn = Math.Min(intReturn, await tskLoop.ConfigureAwait(false));
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
                intReturn = Math.Min(intReturn, await tskLoop.ConfigureAwait(false));
            return intReturn;
        }

        public static async Task<int> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<int>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<int>>(Utils.MaxParallelBatchSize);
            int intReturn = int.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            intReturn = Math.Min(intReturn, await tskLoop.ConfigureAwait(false));
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
                intReturn = Math.Min(intReturn, await tskLoop.ConfigureAwait(false));
            return intReturn;
        }

        public static async Task<int> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, int> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<int> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<int>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = long.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            lngReturn = Math.Min(lngReturn, await tskLoop.ConfigureAwait(false));
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
                lngReturn = Math.Min(lngReturn, await tskLoop.ConfigureAwait(false));
            return lngReturn;
        }

        public static async Task<long> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<long>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<long>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<long>>(Utils.MaxParallelBatchSize);
            long lngReturn = long.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            lngReturn = Math.Min(lngReturn, await tskLoop.ConfigureAwait(false));
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
                lngReturn = Math.Min(lngReturn, await tskLoop.ConfigureAwait(false));
            return lngReturn;
        }

        public static async Task<long> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, long> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<long> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<long>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = float.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            fltReturn = Math.Min(fltReturn, await tskLoop.ConfigureAwait(false));
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
                fltReturn = Math.Min(fltReturn, await tskLoop.ConfigureAwait(false));
            return fltReturn;
        }

        public static async Task<float> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<float>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<float>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<float>>(Utils.MaxParallelBatchSize);
            float fltReturn = float.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            fltReturn = Math.Min(fltReturn, await tskLoop.ConfigureAwait(false));
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
                fltReturn = Math.Min(fltReturn, await tskLoop.ConfigureAwait(false));
            return fltReturn;
        }

        public static async Task<float> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, float> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<float> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<float>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = double.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            dblReturn = Math.Min(dblReturn, await tskLoop.ConfigureAwait(false));
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
                dblReturn = Math.Min(dblReturn, await tskLoop.ConfigureAwait(false));
            return dblReturn;
        }

        public static async Task<double> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<double>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<double>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<double>>(Utils.MaxParallelBatchSize);
            double dblReturn = double.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            dblReturn = Math.Min(dblReturn, await tskLoop.ConfigureAwait(false));
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
                dblReturn = Math.Min(dblReturn, await tskLoop.ConfigureAwait(false));
            return dblReturn;
        }

        public static async Task<double> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, double> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<double> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<double>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = decimal.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            decReturn = Math.Min(decReturn, await tskLoop.ConfigureAwait(false));
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
                decReturn = Math.Min(decReturn, await tskLoop.ConfigureAwait(false));
            return decReturn;
        }

        public static async Task<decimal> MinParallelAsync<T>(this IAsyncEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Task<decimal>> lstTasks = objEnumerable is IAsyncReadOnlyCollection<T> objTemp
                ? new List<Task<decimal>>(Math.Min(Utils.MaxParallelBatchSize, await objTemp.GetCountAsync(token).ConfigureAwait(false)))
                : new List<Task<decimal>>(Utils.MaxParallelBatchSize);
            decimal decReturn = decimal.MaxValue;
            IEnumerator<T> objEnumerator = await objEnumerable.GetEnumeratorAsync(token).ConfigureAwait(false);
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
                            decReturn = Math.Min(decReturn, await tskLoop.ConfigureAwait(false));
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
                decReturn = Math.Min(decReturn, await tskLoop.ConfigureAwait(false));
            return decReturn;
        }

        public static async Task<decimal> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, decimal> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<decimal> MinParallelAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, [NotNull] Func<T, Task<decimal>> funcSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await MinParallelAsync(await tskEnumerable.ConfigureAwait(false), funcPredicate, funcSelector, token).ConfigureAwait(false);
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this IEnumerable<T> objEnumerable, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IEnumerator<T> objEnumerator = objEnumerable is IAsyncEnumerable<T> objEnumerableCast
                ? await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objEnumerable.GetEnumerator();
            try
            {
                if (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    return objEnumerator.Current;
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return default;
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IEnumerator<T> objEnumerator = objEnumerable is IAsyncEnumerable<T> objEnumerableCast
                ? await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objEnumerable.GetEnumerator();
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate.Invoke(objEnumerator.Current))
                        return objEnumerator.Current;
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return default;
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objEnumerable is IAsyncEnumerable<T> objEnumerableCast)
            {
                IEnumerator<T> objEnumerator = await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false);
                try
                {
                    while (objEnumerator.MoveNext())
                    {
                        token.ThrowIfCancellationRequested();
                        if (await funcPredicate.Invoke(objEnumerator.Current).ConfigureAwait(false))
                            return objEnumerator.Current;
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
                foreach (T objItem in objEnumerable)
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate.Invoke(objItem).ConfigureAwait(false))
                        return objItem;
                }
            }

            return default;
        }

        public static async Task<T> FirstOrDefaultAsync<T, T2>(this Task<T2> tskEnumerable, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await FirstOrDefaultAsync(await tskEnumerable.ConfigureAwait(false), token).ConfigureAwait(false);
        }

        public static async Task<T> FirstOrDefaultAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await FirstOrDefaultAsync(lstEnumerableCast, funcPredicate, token).ConfigureAwait(false);
            return await FirstOrDefaultAsync(lstEnumerable, funcPredicate, token).ConfigureAwait(false);
        }

        public static async Task<T> LastOrDefaultAsync<T>(this IEnumerable<T> objEnumerable, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            T objReturn = default;
            IEnumerator<T> objEnumerator = objEnumerable is IAsyncEnumerable<T> objEnumerableCast
                ? await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objEnumerable.GetEnumerator();
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    objReturn = objEnumerator.Current;
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return objReturn;
        }

        public static async Task<T> LastOrDefaultAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            T objReturn = default;
            IEnumerator<T> objEnumerator = objEnumerable is IAsyncEnumerable<T> objEnumerableCast
                ? await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objEnumerable.GetEnumerator();
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (funcPredicate.Invoke(objEnumerator.Current))
                        objReturn = objEnumerator.Current;
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return objReturn;
        }

        public static async Task<T> LastOrDefaultAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            T objReturn = default;
            if (objEnumerable is IAsyncEnumerable<T> objEnumerableCast)
            {
                IEnumerator<T> objEnumerator = await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false);
                try
                {
                    while (objEnumerator.MoveNext())
                    {
                        token.ThrowIfCancellationRequested();
                        if (await funcPredicate.Invoke(objEnumerator.Current).ConfigureAwait(false))
                            objReturn = objEnumerator.Current;
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
                foreach (T objItem in objEnumerable)
                {
                    token.ThrowIfCancellationRequested();
                    if (await funcPredicate.Invoke(objItem).ConfigureAwait(false))
                        objReturn = objItem;
                }
            }

            return objReturn;
        }

        public static async Task<T> LastOrDefaultAsync<T, T2>(this Task<T2> tskEnumerable, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            return await LastOrDefaultAsync(await tskEnumerable.ConfigureAwait(false), token).ConfigureAwait(false);
        }

        public static async Task<T> LastOrDefaultAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> funcPredicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            T2 lstEnumerable = await tskEnumerable.ConfigureAwait(false);
            if (lstEnumerable is IAsyncEnumerable<T> lstEnumerableCast)
                return await LastOrDefaultAsync(lstEnumerableCast, funcPredicate, token).ConfigureAwait(false);
            return await LastOrDefaultAsync(lstEnumerable, funcPredicate, token).ConfigureAwait(false);
        }

        public static Task ForEachAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Action<T> objFuncToRun, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (objEnumerable is IAsyncEnumerable<T> objEnumerableCastOuter)
            {
                return Inner(objEnumerableCastOuter, objFuncToRun, token);
                async Task Inner(IAsyncEnumerable<T> objEnumerableCast, Action<T> objFuncToRunInner, CancellationToken tokenInner)
                {
                    IEnumerator<T> objEnumerator =
                        await objEnumerableCast.GetEnumeratorAsync(tokenInner).ConfigureAwait(false);
                    try
                    {
                        while (objEnumerator.MoveNext())
                        {
                            tokenInner.ThrowIfCancellationRequested();
                            objFuncToRunInner.Invoke(objEnumerator.Current);
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
            }

            foreach (T objItem in objEnumerable)
            {
                if (token.IsCancellationRequested)
                    return Task.FromCanceled(token);
                objFuncToRun.Invoke(objItem);
            }

            return Task.CompletedTask;
        }

        public static async Task ForEachAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task> objFuncToRun, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objEnumerable is IAsyncEnumerable<T> objEnumerableCast)
            {
                IEnumerator<T> objEnumerator = await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false);
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
            else
            {
                foreach (T objItem in objEnumerable)
                {
                    token.ThrowIfCancellationRequested();
                    await objFuncToRun.Invoke(objItem).ConfigureAwait(false);
                }
            }
        }

        public static async Task ForEachAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Action<T> objFuncToRun, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            await ForEachAsync(await tskEnumerable.ConfigureAwait(false), objFuncToRun, token).ConfigureAwait(false);
        }

        public static async Task ForEachAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task> objFuncToRun, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            await ForEachAsync(await tskEnumerable.ConfigureAwait(false), objFuncToRun, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Perform a synchronous action on every element in an enumerable with support for breaking out of the loop.
        /// </summary>
        /// <param name="objEnumerable">Enumerable on which to perform tasks.</param>
        /// <param name="objFuncToRunWithPossibleTerminate">Action to perform. Return true to continue iterating and false to break.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static void ForEachWithBreak<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            foreach (T objItem in objEnumerable)
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
        public static Task ForEachWithBreakAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, bool> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (objEnumerable is IAsyncEnumerable<T> objEnumerableCast)
            {
                return Inner(objEnumerableCast);

                async Task Inner(IAsyncEnumerable<T> objEnumerableInner)
                {
                    IEnumerator<T> objEnumerator = await objEnumerableInner.GetEnumeratorAsync(token).ConfigureAwait(false);
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
            }

            foreach (T objItem in objEnumerable)
            {
                if (token.IsCancellationRequested)
                    return Task.FromCanceled(token);
                if (!objFuncToRunWithPossibleTerminate.Invoke(objItem))
                    break;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Perform an asynchronous action on every element in an enumerable with support for breaking out of the loop.
        /// </summary>
        /// <param name="objEnumerable">Enumerable on which to perform tasks.</param>
        /// <param name="objFuncToRunWithPossibleTerminate">Action to perform. Return true to continue iterating and false to break.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachWithBreakAsync<T>(this IEnumerable<T> objEnumerable, [NotNull] Func<T, Task<bool>> objFuncToRunWithPossibleTerminate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objEnumerable is IAsyncEnumerable<T> objEnumerableCast)
            {
                IEnumerator<T> objEnumerator = await objEnumerableCast.GetEnumeratorAsync(token).ConfigureAwait(false);
                try
                {
                    while (objEnumerator.MoveNext())
                    {
                        token.ThrowIfCancellationRequested();
                        if (!await objFuncToRunWithPossibleTerminate.Invoke(objEnumerator.Current)
                                .ConfigureAwait(false))
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
            else
            {
                foreach (T objItem in objEnumerable)
                {
                    token.ThrowIfCancellationRequested();
                    if (!await objFuncToRunWithPossibleTerminate.Invoke(objItem).ConfigureAwait(false))
                        return;
                }
            }
        }

        /// <summary>
        /// Perform a synchronous action on every element in an enumerable with support for breaking out of the loop.
        /// </summary>
        /// <param name="tskEnumerable">Enumerable on which to perform tasks.</param>
        /// <param name="objFuncToRunWithPossibleTerminate">Action to perform. Return true to continue iterating and false to break.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachWithBreakAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, bool> objFuncToRunWithPossibleTerminate, CancellationToken token = default) where T2 : IAsyncEnumerable<T>
        {
            await ForEachWithBreakAsync(await tskEnumerable.ConfigureAwait(false), objFuncToRunWithPossibleTerminate, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Perform an asynchronous action on every element in an enumerable with support for breaking out of the loop.
        /// </summary>
        /// <param name="tskEnumerable">Enumerable on which to perform tasks.</param>
        /// <param name="objFuncToRunWithPossibleTerminate">Action to perform. Return true to continue iterating and false to break.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ForEachWithBreakAsync<T, T2>(this Task<T2> tskEnumerable, [NotNull] Func<T, Task<bool>> objFuncToRunWithPossibleTerminate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            await ForEachWithBreakAsync(await tskEnumerable.ConfigureAwait(false), objFuncToRunWithPossibleTerminate, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.Aggregate{TSource}(IEnumerable{TSource}, Func{TSource, TSource, TSource})"/>, but deep searches the list, applying the aggregator to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static Task<TSource> DeepAggregateAsync<TSource, T2>(this IEnumerable<TSource> objParentList, Func<TSource, T2> funcGetChildrenMethod, Func<TSource, TSource, TSource> funcAggregate, CancellationToken token = default) where T2 : IAsyncEnumerable<TSource>
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return Task.FromResult<TSource>(default);
            return objParentList.AggregateAsync<TSource, TSource>(default,
                                                                  async (current, objLoopChild) => funcAggregate(
                                                                      funcAggregate(current, objLoopChild),
                                                                      await funcGetChildrenMethod(objLoopChild)
                                                                            .DeepAggregateAsync(
                                                                                funcGetChildrenMethod, funcAggregate, token).ConfigureAwait(false)), token);
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.Aggregate{TSource, TAccumulate}(IEnumerable{TSource}, TAccumulate, Func{TAccumulate, TSource, TAccumulate})"/>, but deep searches the list, applying the aggregator to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<TAccumulate> DeepAggregateAsync<TSource, TAccumulate, T2>(this IEnumerable<TSource> objParentList, Func<TSource, T2> funcGetChildrenMethod, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> funcAggregate, CancellationToken token = default) where T2 : IAsyncEnumerable<TSource>
        {
            token.ThrowIfCancellationRequested();
            return objParentList == null
                ? seed
                : await objParentList.AggregateAsync(seed,
                                                     (current, objLoopChild) => funcGetChildrenMethod(objLoopChild).DeepAggregateAsync(funcGetChildrenMethod,
                                                         funcAggregate(current, objLoopChild), funcAggregate, token), token).ConfigureAwait(false);
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.Aggregate{TSource, TAccumulate, TResult}(IEnumerable{TSource}, TAccumulate, Func{TAccumulate, TSource, TAccumulate}, Func{TAccumulate, TResult})"/>, but deep searches the list, applying the aggregator to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<TResult> DeepAggregateAsync<TSource, TAccumulate, TResult, T2>(this IEnumerable<TSource> objParentList, Func<TSource, T2> funcGetChildrenMethod, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> funcAggregate, Func<TAccumulate, TResult> resultSelector, CancellationToken token = default) where T2 : IAsyncEnumerable<TSource>
        {
            token.ThrowIfCancellationRequested();
            return resultSelector == null
                ? default
                : resultSelector(await objParentList.DeepAggregateAsync(funcGetChildrenMethod, seed, funcAggregate, token).ConfigureAwait(false));
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.All{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static Task<bool> DeepAllAsync<T, T2>([ItemNotNull] this IAsyncEnumerable<T> objParentList, Func<T, T2> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            return objParentList.AllAsync(async objLoopChild =>
            {
                if (!predicate(objLoopChild))
                    return false;
                T2 lstChildren = funcGetChildrenMethod(objLoopChild);
                if (lstChildren is IAsyncEnumerable<T> lstChildrenAsync)
                    return await lstChildrenAsync.DeepAllAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false);
                return lstChildren.DeepAll(funcGetChildrenMethod, predicate);
            }, token);
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.All{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static Task<bool> DeepAllAsync<T, T2>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, T2> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            return objParentList.AllAsync(async objLoopChild =>
            {
                if (!await predicate(objLoopChild).ConfigureAwait(false))
                    return false;
                T2 lstChildren = funcGetChildrenMethod(objLoopChild);
                return await lstChildren.DeepAllAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false);
            }, token);
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.All{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static Task<bool> DeepAllAsync<T, T2>([ItemNotNull] this IEnumerable<T> objParentList,
            Func<T, Task<T2>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default)
            where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            return objParentList.AllAsync(async objLoopChild =>
            {
                if (!predicate(objLoopChild))
                    return false;
                T2 lstChildren = await funcGetChildrenMethod(objLoopChild).ConfigureAwait(false);
                return await lstChildren.DeepAllAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false);
            }, token);
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.All{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static Task<bool> DeepAllAsync<T, T2>([ItemNotNull] this IEnumerable<T> objParentList,
            Func<T, Task<T2>> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default)
            where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            return objParentList.AllAsync(async objLoopChild =>
            {
                if (!await predicate(objLoopChild).ConfigureAwait(false))
                    return false;
                T2 lstChildren = await funcGetChildrenMethod(objLoopChild).ConfigureAwait(false);
                return await lstChildren.DeepAllAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false);
            }, token);
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.Any{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static Task<bool> DeepAnyAsync<T, T2>([ItemNotNull] this IAsyncEnumerable<T> objParentList, Func<T, T2> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            return objParentList.AnyAsync(async objLoopChild =>
            {
                if (predicate(objLoopChild))
                    return true;
                T2 lstChildren = funcGetChildrenMethod(objLoopChild);
                if (lstChildren is IAsyncEnumerable<T> lstChildrenAsync)
                    return await lstChildrenAsync.DeepAnyAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false);
                return lstChildren.DeepAny(funcGetChildrenMethod, predicate);
            }, token);
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.Any{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static Task<bool> DeepAnyAsync<T, T2>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, T2> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            return objParentList.AnyAsync(async objLoopChild =>
            {
                if (await predicate(objLoopChild).ConfigureAwait(false))
                    return true;
                T2 lstChildren = funcGetChildrenMethod(objLoopChild);
                return await lstChildren.DeepAnyAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false);
            }, token);
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.Any{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static Task<bool> DeepAnyAsync<T, T2>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, Task<T2>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            return objParentList.AnyAsync(async objLoopChild =>
            {
                if (predicate(objLoopChild))
                    return true;
                T2 lstChildren = await funcGetChildrenMethod(objLoopChild).ConfigureAwait(false);
                return await lstChildren.DeepAnyAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false);
            }, token);
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.Any{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static Task<bool> DeepAnyAsync<T, T2>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, Task<T2>> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            return objParentList.AnyAsync(async objLoopChild =>
            {
                if (await predicate(objLoopChild).ConfigureAwait(false))
                    return true;
                T2 lstChildren = await funcGetChildrenMethod(objLoopChild).ConfigureAwait(false);
                return await lstChildren.DeepAnyAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false);
            }, token);
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.Count{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<int> DeepCountAsync<T, T2>(this IAsyncEnumerable<T> objParentList, Func<T, T2> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return 0;
            int intReturn = 0;
            IEnumerator<T> objEnumerator = await objParentList.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (predicate(objLoopChild))
                        ++intReturn;
                    T2 lstChildren = funcGetChildrenMethod(objLoopChild);
                    if (lstChildren is IAsyncEnumerable<T> lstChildrenAsync)
                        intReturn += await lstChildrenAsync.DeepCountAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false);
                    else
                        intReturn += lstChildren.DeepCount(funcGetChildrenMethod, predicate);
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

        /// <summary>
        /// Similar to <see cref="Enumerable.Count{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<int> DeepCountAsync<T, T2>(this IEnumerable<T> objParentList, Func<T, T2> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return 0;
            int intReturn = 0;
            IEnumerator<T> objEnumerator = objParentList is IAsyncEnumerable<T> objParentListCast
                ? await objParentListCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objParentList.GetEnumerator();
            try
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (await predicate(objLoopChild).ConfigureAwait(false))
                        ++intReturn;
                    intReturn += await funcGetChildrenMethod(objLoopChild).DeepCountAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false);
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

        /// <summary>
        /// Similar to <see cref="Enumerable.Count{TSource}(IEnumerable{TSource})"/>, but deep searches the list, counting up the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<int> DeepCountAsync<T, T2>(this IEnumerable<T> objParentList, Func<T, T2> funcGetChildrenMethod, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return 0;
            int intReturn = 0;
            IEnumerator<T> objEnumerator = objParentList is IAsyncEnumerable<T> objParentListCast
                ? await objParentListCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objParentList.GetEnumerator();
            try
            {
                while (objEnumerator.MoveNext())
                {
                    intReturn += 1 + await funcGetChildrenMethod(objEnumerator.Current).DeepCountAsync(funcGetChildrenMethod, token).ConfigureAwait(false);
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

        /// <summary>
        /// Similar to <see cref="Enumerable.Count{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<int> DeepCountAsync<T, T2>(this IEnumerable<T> objParentList, Func<T, Task<T2>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return 0;
            int intReturn = 0;
            IEnumerator<T> objEnumerator = objParentList is IAsyncEnumerable<T> objParentListCast
                ? await objParentListCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objParentList.GetEnumerator();
            try
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (predicate(objLoopChild))
                        ++intReturn;
                    intReturn += await (await funcGetChildrenMethod(objLoopChild).ConfigureAwait(false)).DeepCountAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false);
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

        /// <summary>
        /// Similar to <see cref="Enumerable.Count{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<int> DeepCountAsync<T, T2>(this IEnumerable<T> objParentList, Func<T, Task<T2>> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return 0;
            int intReturn = 0;
            IEnumerator<T> objEnumerator = objParentList is IAsyncEnumerable<T> objParentListCast
                ? await objParentListCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objParentList.GetEnumerator();
            try
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (await predicate(objLoopChild).ConfigureAwait(false))
                        ++intReturn;
                    intReturn += await (await funcGetChildrenMethod(objLoopChild).ConfigureAwait(false)).DeepCountAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false);
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

        /// <summary>
        /// Similar to <see cref="Enumerable.Count{TSource}(IEnumerable{TSource})"/>, but deep searches the list, counting up the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<int> DeepCountAsync<T, T2>(this IEnumerable<T> objParentList, Func<T, Task<T2>> funcGetChildrenMethod, CancellationToken token = default) where T2: IAsyncEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return 0;
            int intReturn = 0;
            IEnumerator<T> objEnumerator = objParentList is IAsyncEnumerable<T> objParentListCast
                ? await objParentListCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objParentList.GetEnumerator();
            try
            {
                while (objEnumerator.MoveNext())
                {
                    intReturn += 1 + await (await funcGetChildrenMethod(objEnumerator.Current).ConfigureAwait(false)).DeepCountAsync(funcGetChildrenMethod, token).ConfigureAwait(false);
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

        /// <summary>
        /// Similar to <see cref="Enumerable.First{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepFirstAsync<T, T2>([ItemNotNull] this IAsyncEnumerable<T> objParentList, Func<T, T2> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            IEnumerator<T> objEnumerator = await objParentList.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (predicate(objLoopChild))
                        return objLoopChild;
                    try
                    {
                        T2 lstChildren = funcGetChildrenMethod(objLoopChild);
                        if (lstChildren is IAsyncEnumerable<T> lstChildrenAsync)
                            return await lstChildrenAsync.DeepFirstAsync(funcGetChildrenMethod, predicate, token)
                                .ConfigureAwait(false);
                        else
                            return lstChildren.DeepFirst(funcGetChildrenMethod, predicate, token);
                    }
                    catch (InvalidOperationException)
                    {
                        //swallow this
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
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.First{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepFirstAsync<T, T2>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, Task<T2>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            IEnumerator<T> objEnumerator = objParentList is IAsyncEnumerable<T> objParentListCast
                ? await objParentListCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objParentList.GetEnumerator();
            try
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (predicate(objLoopChild))
                        return objLoopChild;
                    try
                    {
                        return await (await funcGetChildrenMethod(objLoopChild).ConfigureAwait(false)).DeepFirstAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false);
                    }
                    catch (InvalidOperationException)
                    {
                        //swallow this
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
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepFirstOrDefaultAsync<T, T2>(this IAsyncEnumerable<T> objParentList, Func<T, T2> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            IEnumerator<T> objEnumerator = await objParentList.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (predicate(objLoopChild))
                        return objLoopChild;
                    T objReturn;
                    T2 lstChildren = funcGetChildrenMethod(objLoopChild);
                    if (lstChildren is IAsyncEnumerable<T> lstChildrenAsync)
                        objReturn = await lstChildrenAsync.DeepFirstOrDefaultAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false);
                    else
                        objReturn = lstChildren.DeepFirstOrDefault(funcGetChildrenMethod, predicate, token);
                    if (objReturn?.Equals(default(T)) == false)
                        return objReturn;
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return default;
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepFirstOrDefaultAsync<T, T2>(this IEnumerable<T> objParentList, Func<T, T2> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            IEnumerator<T> objEnumerator = objParentList is IAsyncEnumerable<T> objParentListCast
                ? await objParentListCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objParentList.GetEnumerator();
            try
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (await predicate(objLoopChild).ConfigureAwait(false))
                        return objLoopChild;
                    T objReturn = await funcGetChildrenMethod(objLoopChild).DeepFirstOrDefaultAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false);
                    if (objReturn?.Equals(default(T)) == false)
                        return objReturn;
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return default;
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepFirstOrDefaultAsync<T, T2>(this IEnumerable<T> objParentList, Func<T, Task<T2>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            IEnumerator<T> objEnumerator = objParentList is IAsyncEnumerable<T> objParentListCast
                ? await objParentListCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objParentList.GetEnumerator();
            try
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (predicate(objLoopChild))
                        return objLoopChild;
                    T objReturn = await (await funcGetChildrenMethod(objLoopChild).ConfigureAwait(false)).DeepFirstOrDefaultAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false);
                    if (objReturn?.Equals(default(T)) == false)
                        return objReturn;
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return default;
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepFirstOrDefaultAsync<T, T2>(this IEnumerable<T> objParentList, Func<T, Task<T2>> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            IEnumerator<T> objEnumerator = objParentList is IAsyncEnumerable<T> objParentListCast
                ? await objParentListCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objParentList.GetEnumerator();
            try
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (await predicate(objLoopChild).ConfigureAwait(false))
                        return objLoopChild;
                    T objReturn = await (await funcGetChildrenMethod(objLoopChild).ConfigureAwait(false)).DeepFirstOrDefaultAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false);
                    if (objReturn?.Equals(default(T)) == false)
                        return objReturn;
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return default;
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.Last{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastAsync<T, T2>([ItemNotNull] this IAsyncEnumerable<T> objParentList, Func<T, T2> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                throw new InvalidOperationException();
            bool blnFoundValue = false;
            T objReturn = default;
            IEnumerator<T> objEnumerator = await objParentList.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (predicate(objLoopChild))
                    {
                        objReturn = objLoopChild;
                        blnFoundValue = true;
                    }
                    T objTemp;
                    try
                    {
                        T2 lstChildren = funcGetChildrenMethod(objLoopChild);
                        if (lstChildren is IAsyncEnumerable<T> lstChildrenAsync)
                            objTemp = await lstChildrenAsync.DeepLastAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false);
                        else
                            objTemp = lstChildren.DeepLast(funcGetChildrenMethod, predicate);
                        blnFoundValue = true;
                    }
                    catch (InvalidOperationException)
                    {
                        continue;
                    }
                    objReturn = objTemp;
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            if (blnFoundValue)
                return objReturn;
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.Last{TSource}(IEnumerable{TSource})"/>, but deep searches the list, returning the last element out of the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastAsync<T, T2>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, T2> funcGetChildrenMethod, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                throw new InvalidOperationException();
            bool blnFoundValue = false;
            T objReturn = default;
            IEnumerator<T> objEnumerator = objParentList is IAsyncEnumerable<T> objParentListCast
                ? await objParentListCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objParentList.GetEnumerator();
            try
            {
                while (objEnumerator.MoveNext())
                {
                    blnFoundValue = true;
                    objReturn = objEnumerator.Current;
                }
                try
                {
                    T objTemp = await funcGetChildrenMethod(objReturn).DeepLastAsync(funcGetChildrenMethod, token).ConfigureAwait(false);
                    objReturn = objTemp;
                }
                catch (InvalidOperationException)
                {
                    //swallow this
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            if (blnFoundValue)
                return objReturn;
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.Last{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastAsync<T, T2>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, Task<T2>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                throw new InvalidOperationException();
            bool blnFoundValue = false;
            T objReturn = default;
            IEnumerator<T> objEnumerator = objParentList is IAsyncEnumerable<T> objParentListCast
                ? await objParentListCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objParentList.GetEnumerator();
            try
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (predicate(objLoopChild))
                    {
                        objReturn = objLoopChild;
                        blnFoundValue = true;
                    }
                    T objTemp;
                    try
                    {
                        objTemp = await (await funcGetChildrenMethod(objLoopChild).ConfigureAwait(false)).DeepLastAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false);
                        blnFoundValue = true;
                    }
                    catch (InvalidOperationException)
                    {
                        continue;
                    }
                    objReturn = objTemp;
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            if (blnFoundValue)
                return objReturn;
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.Last{TSource}(IEnumerable{TSource})"/>, but deep searches the list, returning the last element out of the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastAsync<T, T2>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, Task<T2>> funcGetChildrenMethod, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                throw new InvalidOperationException();
            bool blnFoundValue = false;
            T objReturn = default;
            IEnumerator<T> objEnumerator = objParentList is IAsyncEnumerable<T> objParentListCast
                ? await objParentListCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objParentList.GetEnumerator();
            try
            {
                while (objEnumerator.MoveNext())
                {
                    blnFoundValue = true;
                    objReturn = objEnumerator.Current;
                }
                try
                {
                    T objTemp = await (await funcGetChildrenMethod(objReturn).ConfigureAwait(false)).DeepLastAsync(funcGetChildrenMethod, token).ConfigureAwait(false);
                    objReturn = objTemp;
                }
                catch (InvalidOperationException)
                {
                    //swallow this
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            if (blnFoundValue)
                return objReturn;
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.LastOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastOrDefaultAsync<T, T2>(this IAsyncEnumerable<T> objParentList, Func<T, T2> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            T objReturn = default;
            IEnumerator<T> objEnumerator = await objParentList.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (predicate(objLoopChild))
                        objReturn = objLoopChild;
                    T objTemp;
                    T2 lstChildren = funcGetChildrenMethod(objLoopChild);
                    if (lstChildren is IAsyncEnumerable<T> lstChildrenAsync)
                        objTemp = await lstChildrenAsync.DeepLastOrDefaultAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false);
                    else
                        objTemp = lstChildren.DeepLastOrDefault(funcGetChildrenMethod, predicate);
                    if (objTemp?.Equals(default(T)) == false)
                        objReturn = objTemp;
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return objReturn;
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.LastOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastOrDefaultAsync<T, T2>(this IEnumerable<T> objParentList, Func<T, T2> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            T objReturn = default;
            IEnumerator<T> objEnumerator = objParentList is IAsyncEnumerable<T> objParentListCast
                ? await objParentListCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objParentList.GetEnumerator();
            try
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (await predicate(objLoopChild).ConfigureAwait(false))
                        objReturn = objLoopChild;
                    T objTemp = await funcGetChildrenMethod(objLoopChild).DeepLastOrDefaultAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false);
                    if (objTemp?.Equals(default(T)) == false)
                        objReturn = objTemp;
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return objReturn;
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.LastOrDefault{TSource}(IEnumerable{TSource})"/>, but deep searches the list, returning the last element out of the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastOrDefaultAsync<T, T2>(this IEnumerable<T> objParentList, Func<T, T2> funcGetChildrenMethod, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            T objReturn = objParentList is IAsyncEnumerable<T> objParentListCast
                ? await objParentListCast.LastOrDefaultAsync(token).ConfigureAwait(false)
                : objParentList.LastOrDefault();
            if (funcGetChildrenMethod != null)
            {
                T2 lstChildrenRaw = funcGetChildrenMethod(objReturn);
                List<T> lstChildren = lstChildrenRaw is IAsyncEnumerable<T> lstChildrenRawAsync
                    ? await lstChildrenRawAsync.ToListAsync(token).ConfigureAwait(false)
                    : lstChildrenRaw.ToList();
                if (lstChildren.Count > 0)
                {
                    T objTemp = await lstChildren.DeepLastOrDefaultAsync(funcGetChildrenMethod, token).ConfigureAwait(false);
                    if (objTemp?.Equals(default(T)) == false)
                        return objTemp;
                }
            }
            return objReturn;
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.LastOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastOrDefaultAsync<T, T2>(this IEnumerable<T> objParentList, Func<T, Task<T2>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            T objReturn = default;
            IEnumerator<T> objEnumerator = objParentList is IAsyncEnumerable<T> objParentListCast
                ? await objParentListCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objParentList.GetEnumerator();
            try
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (predicate(objLoopChild))
                        objReturn = objLoopChild;
                    T objTemp = await (await funcGetChildrenMethod(objLoopChild).ConfigureAwait(false)).DeepLastOrDefaultAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false);
                    if (objTemp?.Equals(default(T)) == false)
                        objReturn = objTemp;
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return objReturn;
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.LastOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastOrDefaultAsync<T, T2>(this IEnumerable<T> objParentList, Func<T, Task<T2>> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            T objReturn = default;
            IEnumerator<T> objEnumerator = objParentList is IAsyncEnumerable<T> objParentListCast
                ? await objParentListCast.GetEnumeratorAsync(token).ConfigureAwait(false)
                : objParentList.GetEnumerator();
            try
            {
                while (objEnumerator.MoveNext())
                {
                    T objLoopChild = objEnumerator.Current;
                    if (await predicate(objLoopChild).ConfigureAwait(false))
                        objReturn = objLoopChild;
                    T objTemp = await (await funcGetChildrenMethod(objLoopChild).ConfigureAwait(false)).DeepLastOrDefaultAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false);
                    if (objTemp?.Equals(default(T)) == false)
                        objReturn = objTemp;
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return objReturn;
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.LastOrDefault{TSource}(IEnumerable{TSource})"/>, but deep searches the list, returning the last element out of the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<T> DeepLastOrDefaultAsync<T, T2>(this IEnumerable<T> objParentList, Func<T, Task<T2>> funcGetChildrenMethod, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            if (objParentList == null)
                return default;
            T objReturn = objParentList is IAsyncEnumerable<T> objParentListCast
                ? await objParentListCast.LastOrDefaultAsync(token).ConfigureAwait(false)
                : objParentList.LastOrDefault();
            if (funcGetChildrenMethod != null)
            {
                T2 lstChildrenRaw = await funcGetChildrenMethod(objReturn).ConfigureAwait(false);
                List<T> lstChildren = lstChildrenRaw is IAsyncEnumerable<T> lstChildrenRaw2
                    ? await lstChildrenRaw2.ToListAsync(token).ConfigureAwait(false)
                    : lstChildrenRaw.ToList();
                if (lstChildren.Count > 0)
                {
                    T objTemp = await lstChildren.DeepLastOrDefaultAsync(funcGetChildrenMethod, token).ConfigureAwait(false);
                    if (objTemp?.Equals(default(T)) == false)
                        return objTemp;
                }
            }
            return objReturn;
        }

        /// <summary>
        /// Gets all relatives in the list, including the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static IEnumerable<T> GetAllDescendants<T, T2>([ItemNotNull] this IAsyncEnumerable<T> objParentList, Func<T, T2> funcGetChildrenMethod, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            foreach (T objItem in objParentList)
            {
                token.ThrowIfCancellationRequested();
                yield return objItem;
                token.ThrowIfCancellationRequested();
                foreach (T objLoopGrandchild in funcGetChildrenMethod(objItem).GetAllDescendants(funcGetChildrenMethod))
                {
                    token.ThrowIfCancellationRequested();
                    yield return objLoopGrandchild;
                }
            }
        }

        /// <summary>
        /// Gets all relatives in the list, including the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<List<T>> GetAllDescendantsAsync<T, T2>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, T2> funcGetChildrenMethod, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            List<T> lstReturn = new List<T>();
            await objParentList.ForEachAsync(async objLoopChild =>
            {
                lstReturn.Add(objLoopChild);
                token.ThrowIfCancellationRequested();
                T2 lstChildren = funcGetChildrenMethod(objLoopChild);
                if (lstChildren is IAsyncEnumerable<T> lstChildrenCast)
                    lstReturn.AddRange(await lstChildrenCast.GetAllDescendantsAsync(funcGetChildrenMethod, token).ConfigureAwait(false));
                else
                    lstReturn.AddRange(lstChildren.GetAllDescendants(funcGetChildrenMethod));
            }, token).ConfigureAwait(false);
            return lstReturn;
        }

        /// <summary>
        /// Gets all relatives in the list, including the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<List<T>> GetAllDescendantsAsync<T, T2>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, Task<T2>> funcGetChildrenMethod, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            List<T> lstReturn = new List<T>();
            if (objParentList is IAsyncEnumerable<T> objParentListCast)
            {
                await objParentListCast.ForEachAsync(async objLoopChild =>
                {
                    lstReturn.Add(objLoopChild);
                    token.ThrowIfCancellationRequested();
                    T2 lstChildren = await funcGetChildrenMethod(objLoopChild).ConfigureAwait(false);
                    if (lstChildren is IAsyncEnumerable<T> lstChildrenCast)
                        lstReturn.AddRange(await lstChildrenCast.GetAllDescendantsAsync(funcGetChildrenMethod, token)
                            .ConfigureAwait(false));
                    else
                        lstReturn.AddRange(await lstChildren.GetAllDescendantsAsync(funcGetChildrenMethod, token)
                            .ConfigureAwait(false));
                }, token).ConfigureAwait(false);
            }
            else
            {
                foreach (T objLoopChild in objParentList)
                {
                    token.ThrowIfCancellationRequested();
                    lstReturn.Add(objLoopChild);
                    token.ThrowIfCancellationRequested();
                    T2 lstChildren = await funcGetChildrenMethod(objLoopChild).ConfigureAwait(false);
                    if (lstChildren is IAsyncEnumerable<T> lstChildrenCast)
                        lstReturn.AddRange(await lstChildrenCast.GetAllDescendantsAsync(funcGetChildrenMethod, token)
                            .ConfigureAwait(false));
                    else
                        lstReturn.AddRange(await lstChildren.GetAllDescendantsAsync(funcGetChildrenMethod, token)
                            .ConfigureAwait(false));
                }
            }
            return lstReturn;
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.Where{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static IEnumerable<T> DeepWhere<T, T2>([ItemNotNull] this IAsyncEnumerable<T> objParentList, Func<T, T2> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            foreach (T objItem in objParentList)
            {
                token.ThrowIfCancellationRequested();
                T objLoopCurrent = objItem;
                if (predicate.Invoke(objLoopCurrent))
                    yield return objLoopCurrent;
                token.ThrowIfCancellationRequested();
                foreach (T objLoopGrandchild in funcGetChildrenMethod(objLoopCurrent).DeepWhere(funcGetChildrenMethod, predicate))
                {
                    token.ThrowIfCancellationRequested();
                    yield return objLoopGrandchild;
                }
            }
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.Where{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<List<T>> DeepWhereAsync<T, T2>([ItemNotNull] this IAsyncEnumerable<T> objParentList, Func<T, T2> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            List<T> lstReturn = new List<T>();
            await objParentList.ForEachAsync(async objLoopChild =>
            {
                if (predicate(objLoopChild))
                    lstReturn.Add(objLoopChild);
                token.ThrowIfCancellationRequested();
                T2 lstChildren = funcGetChildrenMethod(objLoopChild);
                if (lstChildren is IAsyncEnumerable<T> lstChildrenCast)
                    lstReturn.AddRange(await lstChildrenCast.DeepWhereAsync(funcGetChildrenMethod, predicate, token).ConfigureAwait(false));
                else
                    lstReturn.AddRange(lstChildren.DeepWhere(funcGetChildrenMethod, predicate));
            }, token).ConfigureAwait(false);
            return lstReturn;
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.Where{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<List<T>> DeepWhereAsync<T, T2>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, Task<T2>> funcGetChildrenMethod, Func<T, bool> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            List<T> lstReturn = new List<T>();
            await objParentList.ForEachAsync(async objLoopChild =>
            {
                if (predicate(objLoopChild))
                    lstReturn.Add(objLoopChild);
                token.ThrowIfCancellationRequested();
                T2 lstChildren = await funcGetChildrenMethod(objLoopChild).ConfigureAwait(false);
                if (lstChildren is IAsyncEnumerable<T> lstChildrenCast)
                    lstReturn.AddRange(await lstChildrenCast.DeepWhereAsync(funcGetChildrenMethod, predicate, token)
                        .ConfigureAwait(false));
                else
                    lstReturn.AddRange(await lstChildren.DeepWhereAsync(funcGetChildrenMethod, predicate, token)
                        .ConfigureAwait(false));
            }, token).ConfigureAwait(false);
            return lstReturn;
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.Where{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<List<T>> DeepWhereAsync<T, T2>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, T2> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            List<T> lstReturn = new List<T>();
            await objParentList.ForEachAsync(async objLoopChild =>
            {
                if (await predicate(objLoopChild).ConfigureAwait(false))
                    lstReturn.Add(objLoopChild);
                token.ThrowIfCancellationRequested();
                T2 lstChildren = funcGetChildrenMethod(objLoopChild);
                if (lstChildren is IAsyncEnumerable<T> lstChildrenCast)
                    lstReturn.AddRange(await lstChildrenCast.DeepWhereAsync(funcGetChildrenMethod, predicate, token)
                        .ConfigureAwait(false));
                else
                    lstReturn.AddRange(await lstChildren.DeepWhereAsync(funcGetChildrenMethod, predicate, token)
                        .ConfigureAwait(false));
            }, token).ConfigureAwait(false);
            return lstReturn;
        }

        /// <summary>
        /// Similar to <see cref="Enumerable.Where{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, but deep searches the list, applying the predicate to the parents, the parents' children, their children's children, etc.
        /// </summary>
        public static async Task<List<T>> DeepWhereAsync<T, T2>([ItemNotNull] this IEnumerable<T> objParentList, Func<T, Task<T2>> funcGetChildrenMethod, Func<T, Task<bool>> predicate, CancellationToken token = default) where T2 : IEnumerable<T>
        {
            token.ThrowIfCancellationRequested();
            List<T> lstReturn = new List<T>();
            await objParentList.ForEachAsync(async objLoopChild =>
            {
                if (await predicate(objLoopChild).ConfigureAwait(false))
                    lstReturn.Add(objLoopChild);
                token.ThrowIfCancellationRequested();
                T2 lstChildren = await funcGetChildrenMethod(objLoopChild).ConfigureAwait(false);
                if (lstChildren is IAsyncEnumerable<T> lstChildrenCast)
                    lstReturn.AddRange(await lstChildrenCast.DeepWhereAsync(funcGetChildrenMethod, predicate, token)
                        .ConfigureAwait(false));
                else
                    lstReturn.AddRange(await lstChildren.DeepWhereAsync(funcGetChildrenMethod, predicate, token)
                        .ConfigureAwait(false));
            }, token).ConfigureAwait(false);
            return lstReturn;
        }
    }
}
