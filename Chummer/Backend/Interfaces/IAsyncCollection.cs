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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Chummer.Annotations;

namespace Chummer
{
    public interface IAsyncCollection<T> : ICollection<T>, IAsyncEnumerable<T>
    {
        Task<int> GetCountAsync(CancellationToken token = default);

        Task AddAsync(T item, CancellationToken token = default);

        Task ClearAsync(CancellationToken token = default);

        Task<bool> ContainsAsync(T item, CancellationToken token = default);

        Task CopyToAsync(T[] array, int index, CancellationToken token = default);

        Task<bool> RemoveAsync(T item, CancellationToken token = default);
    }

    public static class AsyncCollectionExtensions
    {
        public static async Task AddRangeAsync<T>(this IAsyncCollection<T> lstCollection, IEnumerable<T> lstToAdd, CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            foreach (T objItem in lstToAdd)
                await lstCollection.AddAsync(objItem, token).ConfigureAwait(false);
        }

        public static async Task AddRangeAsync<T>(this IAsyncCollection<T> lstCollection, IAsyncEnumerable<T> lstToAdd, CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            IEnumerator<T> objEnumerator = await lstToAdd.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    await lstCollection.AddAsync(objEnumerator.Current, token).ConfigureAwait(false);
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
        /// Get a HashCode representing the contents of a collection in a way where the order of the items is irrelevant.
        /// Uses the parallel option for large enough collections where it could potentially be faster
        /// NOTE: GetEnsembleHashCode and GetOrderInvariantEnsembleHashCode will almost never be the same for the same collection!
        /// </summary>
        /// <typeparam name="T">The type for which <see cref="object.GetHashCode"/> will be called</typeparam>
        /// <param name="lstItems">The collection containing the contents</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>A HashCode that is generated based on the contents of <paramref name="lstItems"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<int> GetOrderInvariantEnsembleHashCodeSmartAsync<T>(this IAsyncReadOnlyCollection<T> lstItems, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return await lstItems.GetCountAsync(token).ConfigureAwait(false) > ushort.MaxValue
                ? await lstItems.GetOrderInvariantEnsembleHashCodeParallelAsync(token).ConfigureAwait(false)
                : await lstItems.GetOrderInvariantEnsembleHashCodeAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        /// Syntactic sugar for a version of SequenceEquals that does not care about the order of elements, just the two collections' contents.
        /// </summary>
        /// <param name="first">First collection to compare.</param>
        /// <param name="second">Second collection to compare.</param>
        /// <returns>True if <paramref name="first"/> and <paramref name="second"/> are of the same size and have the same contents, false otherwise.</returns>
        public static async Task<bool> CollectionEqualAsync<T>([NotNull] this IAsyncReadOnlyCollection<T> first, [NotNull] IReadOnlyCollection<T> second, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (second is IAsyncReadOnlyCollection<T> secondAsync)
            {
                if (await first.GetCountAsync(token).ConfigureAwait(false) != await secondAsync.GetCountAsync(token).ConfigureAwait(false))
                    return false;
                // Use built-in, faster implementations if they are available
                if (first is IAsyncSet<T> setFirst2)
                    return await setFirst2.SetEqualsAsync(second, token).ConfigureAwait(false);
                if (second is IAsyncSet<T> setSecondAsync)
                    return await setSecondAsync.SetEqualsAsync(first, token).ConfigureAwait(false);
                if (await first.GetOrderInvariantEnsembleHashCodeSmartAsync(token).ConfigureAwait(false)
                    != await secondAsync.GetOrderInvariantEnsembleHashCodeSmartAsync(token).ConfigureAwait(false))
                    return false;
                List<T> lstTemp2 = await secondAsync.ToListAsync(token).ConfigureAwait(false);
                IEnumerator<T> objEnumerator2 = await first.GetEnumeratorAsync(token).ConfigureAwait(false);
                try
                {
                    if (objEnumerator2.MoveNext() && !lstTemp2.Remove(objEnumerator2.Current))
                        return false;
                }
                finally
                {
                    if (objEnumerator2 is IAsyncDisposable objAsyncDisposable)
                        await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                    else
                        objEnumerator2.Dispose();
                }
                return lstTemp2.Count == 0; // The list will only be empty if all elements in second are also in first
            }
            if (await first.GetCountAsync(token).ConfigureAwait(false) != second.Count)
                return false;
            // Use built-in, faster implementations if they are available
            if (first is IAsyncSet<T> setFirst)
                return await setFirst.SetEqualsAsync(second, token).ConfigureAwait(false);
            if (second is ISet<T> setSecond)
                return setSecond.SetEquals(first);
            if (await first.GetOrderInvariantEnsembleHashCodeSmartAsync(token).ConfigureAwait(false) != second.GetOrderInvariantEnsembleHashCodeSmart(token))
                return false;
            List<T> lstTemp = second.ToList();
            IEnumerator<T> objEnumerator = await first.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                if (objEnumerator.MoveNext() && !lstTemp.Remove(objEnumerator.Current))
                    return false;
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return lstTemp.Count == 0; // The list will only be empty if all elements in second are also in first
        }

        /// <summary>
        /// Syntactic sugar for a version of SequenceEquals that does not care about the order of elements, just the two collections' contents.
        /// </summary>
        /// <param name="first">First collection to compare.</param>
        /// <param name="second">Second collection to compare.</param>
        /// <param name="comparer">Special equality comparer to use instead of the default one.</param>
        /// <returns>True if <paramref name="first"/> and <paramref name="second"/> are of the same size and have the same contents, false otherwise.</returns>
        public static async Task<bool> CollectionEqualAsync<T>([NotNull] this IAsyncReadOnlyCollection<T> first, [NotNull] IReadOnlyCollection<T> second, IEqualityComparer<T> comparer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (second is IAsyncReadOnlyCollection<T> secondAsync)
            {
                // Sets do not have IEqualityComparer versions for SetEquals, so we always need to do this the slow way
                if (await first.GetCountAsync(token).ConfigureAwait(false) != await secondAsync.GetCountAsync(token).ConfigureAwait(false))
                    return false;
                // Cannot use hashes because the equality comparer might not be compatible with them (it could mark two objects with different hashes equal)
                List<T> lstTemp2 = second.ToList();
                IEnumerator<T> objEnumerator2 = await first.GetEnumeratorAsync(token).ConfigureAwait(false);
                try
                {
                    if (objEnumerator2.MoveNext())
                    {
                        T item = objEnumerator2.Current;
                        int i = lstTemp2.FindIndex(x => comparer.Equals(x, item));
                        if (i < 0)
                            return false;
                        lstTemp2.RemoveAt(i);
                    }
                }
                finally
                {
                    if (objEnumerator2 is IAsyncDisposable objAsyncDisposable)
                        await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                    else
                        objEnumerator2.Dispose();
                }
                return lstTemp2.Count == 0; // The list will only be empty if all elements in second are also in first
            }
            // Sets do not have IEqualityComparer versions for SetEquals, so we always need to do this the slow way
            if (await first.GetCountAsync(token).ConfigureAwait(false) != second.Count)
                return false;
            // Cannot use hashes because the equality comparer might not be compatible with them (it could mark two objects with different hashes equal)
            List<T> lstTemp = second.ToList();
            IEnumerator<T> objEnumerator = await first.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                if (objEnumerator.MoveNext())
                {
                    T item = objEnumerator.Current;
                    int i = lstTemp.FindIndex(x => comparer.Equals(x, item));
                    if (i < 0)
                        return false;
                    lstTemp.RemoveAt(i);
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            return lstTemp.Count == 0; // The list will only be empty if all elements in second are also in first
        }
    }
}
