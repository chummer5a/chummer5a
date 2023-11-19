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

namespace Chummer
{
    public interface IAsyncReadOnlyList<T> : IAsyncReadOnlyCollection<T>, IReadOnlyList<T>
    {
        Task<T> GetValueAtAsync(int index, CancellationToken token = default);
    }

    public static class AsyncReadOnlyListExtensions
    {
        public static async Task<int> BinarySearchAsync<T>(this IAsyncReadOnlyList<T> lstCollection, T objItem, CancellationToken token = default) where T : IComparable
        {
            int intLastIntervalBounds = await lstCollection.GetCountAsync(token).ConfigureAwait(false) - 1;
            int intBase = 0;
            for (int i = intLastIntervalBounds / 2; i > 0; i = intLastIntervalBounds / 2)
            {
                int intLoopIndex = intBase + i;
                int intCompareResult = objItem.CompareTo(await lstCollection.GetValueAtAsync(intLoopIndex, token).ConfigureAwait(false));
                if (intCompareResult == 0)
                    return intLoopIndex;
                if (intCompareResult > 0)
                {
                    intBase += intLastIntervalBounds - i;
                    intLastIntervalBounds -= i; // Makes sure that for odd sizes, we end up spanning every item
                }
                else
                {
                    intLastIntervalBounds = i;
                }
            }

            return ~(intBase + 1); // Bitwise complement of next item larger than this one, just like List.BinarySearch
        }

        public static async Task<int> BinarySearchAsync<T>(this IAsyncReadOnlyList<T> lstCollection, T objItem, IComparer<T> comparer, CancellationToken token = default)
        {
            int intLastIntervalBounds = await lstCollection.GetCountAsync(token).ConfigureAwait(false) - 1;
            int intBase = 0;
            for (int i = intLastIntervalBounds / 2; i > 0; i = intLastIntervalBounds / 2)
            {
                int intLoopIndex = intBase + i;
                int intCompareResult = comparer.Compare(objItem, await lstCollection.GetValueAtAsync(intLoopIndex, token).ConfigureAwait(false));
                if (intCompareResult == 0)
                    return intLoopIndex;
                if (intCompareResult > 0)
                {
                    intBase += intLastIntervalBounds - i;
                    intLastIntervalBounds -= i; // Makes sure that for odd sizes, we end up spanning every item
                }
                else
                {
                    intLastIntervalBounds = i;
                }
            }

            return ~(intBase + 1); // Bitwise complement of next item larger than this one, just like List.BinarySearch
        }

        public static async Task<int> BinarySearchAsync<T>(this IAsyncReadOnlyList<T> lstCollection, int index, int count, T objItem, IComparer<T> comparer, CancellationToken token = default)
        {
            int intLastIntervalBounds = count - 1;
            int intBase = index;
            for (int i = intLastIntervalBounds / 2; i > 0; i = intLastIntervalBounds / 2)
            {
                int intLoopIndex = intBase + i;
                int intCompareResult = comparer.Compare(objItem, await lstCollection.GetValueAtAsync(intLoopIndex, token).ConfigureAwait(false));
                if (intCompareResult == 0)
                    return intLoopIndex;
                if (intCompareResult > 0)
                {
                    intBase += intLastIntervalBounds - i;
                    intLastIntervalBounds -= i; // Makes sure that for odd sizes, we end up spanning every item
                }
                else
                {
                    intLastIntervalBounds = i;
                }
            }

            return ~(intBase + 1); // Bitwise complement of next item larger than this one, just like List.BinarySearch
        }

        public static Task<int> FindIndexAsync<T>(this IAsyncReadOnlyList<T> lstCollection, Predicate<T> predicate, CancellationToken token = default)
        {
            return FindIndexAsync(lstCollection, 0, predicate, token);
        }

        public static async Task<int> FindIndexAsync<T>(this IAsyncReadOnlyList<T> lstCollection, int startIndex, Predicate<T> predicate, CancellationToken token = default)
        {
            for (int i = startIndex; i < await lstCollection.GetCountAsync(token).ConfigureAwait(false); ++i)
            {
                if (predicate(await lstCollection.GetValueAtAsync(i, token).ConfigureAwait(false)))
                    return i;
            }
            return -1;
        }

        public static async Task<int> FindIndexAsync<T>(this IAsyncReadOnlyList<T> lstCollection, int startIndex, int count, Predicate<T> predicate, CancellationToken token = default)
        {
            int intUpperBounds = count - startIndex;
            for (int i = startIndex; i < Math.Min(await lstCollection.GetCountAsync(token).ConfigureAwait(false), intUpperBounds); ++i)
            {
                if (predicate(await lstCollection.GetValueAtAsync(i, token).ConfigureAwait(false)))
                    return i;
            }
            return -1;
        }

        public static async Task<int> FindLastIndexAsync<T>(this IAsyncReadOnlyList<T> lstCollection, Predicate<T> predicate, CancellationToken token = default)
        {
            for (int i = await lstCollection.GetCountAsync(token).ConfigureAwait(false) - 1; i >= 0; --i)
            {
                if (predicate(await lstCollection.GetValueAtAsync(i, token).ConfigureAwait(false)))
                    return i;
            }
            return -1;
        }

        public static async Task<int> FindLastIndexAsync<T>(this IAsyncReadOnlyList<T> lstCollection, int startIndex, Predicate<T> predicate, CancellationToken token = default)
        {
            for (int i = startIndex; i >= 0; --i)
            {
                if (predicate(await lstCollection.GetValueAtAsync(i, token).ConfigureAwait(false)))
                    return i;
            }
            return -1;
        }

        public static async Task<int> FindLastIndexAsync<T>(this IAsyncReadOnlyList<T> lstCollection, int startIndex, int count, Predicate<T> predicate, CancellationToken token = default)
        {
            int intLowerBounds = startIndex - count;
            for (int i = startIndex; i >= Math.Max(0, intLowerBounds); --i)
            {
                if (predicate(await lstCollection.GetValueAtAsync(i, token).ConfigureAwait(false)))
                    return i;
            }
            return -1;
        }

        public static async Task<int> LastIndexOfAsync<T>(this IAsyncReadOnlyList<T> lstCollection, T objItem, CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (objItem == null)
                throw new ArgumentNullException(nameof(objItem));
            for (int i = await lstCollection.GetCountAsync(token).ConfigureAwait(false) - 1; i >= 0; --i)
            {
                if ((await lstCollection.GetValueAtAsync(i, token).ConfigureAwait(false)).Equals(objItem))
                {
                    return i;
                }
            }

            return -1;
        }

        public static async Task<int> IndexOfAsync<T>(this IAsyncReadOnlyList<T> lstCollection, T objItem, CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (objItem == null)
                throw new ArgumentNullException(nameof(objItem));
            for (int i = 0; i < await lstCollection.GetCountAsync(token).ConfigureAwait(false); ++i)
            {
                if ((await lstCollection.GetValueAtAsync(i, token).ConfigureAwait(false)).Equals(objItem))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
