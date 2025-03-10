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
    public static class ListExtensions
    {
        public static void AddWithSort<T>(this IList<T> lstCollection, T objNewItem,
            Action<T, T> funcOverrideIfEquals = null, CancellationToken token = default) where T : IComparable
        {
            token.ThrowIfCancellationRequested();
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            IDisposable objLocker = null;
            if (lstCollection is IHasLockObject objHasLock)
                objLocker = objHasLock.LockObject.EnterUpgradeableReadLock(token);
            else
                objHasLock = null;
            try
            {
                token.ThrowIfCancellationRequested();
                // Binary search for the place where item should be inserted
                int intIntervalEnd = lstCollection.Count - 1;
                int intTargetIndex = intIntervalEnd / 2;
                for (int intIntervalStart = 0;
                     intIntervalStart <= intIntervalEnd;
                     intTargetIndex = (intIntervalStart + intIntervalEnd) / 2)
                {
                    token.ThrowIfCancellationRequested();
                    T objLoopExistingItem = lstCollection[intTargetIndex];
                    int intCompareResult = objLoopExistingItem.CompareTo(objNewItem);
                    if (intCompareResult == 0)
                    {
                        // Make sure we insert new items at the end of any equalities (so that order is maintained when adding multiple items)
                        for (int i = intTargetIndex + 1; i < lstCollection.Count; ++i)
                        {
                            T objInnerLoopExistingItem = lstCollection[i];
                            if (objInnerLoopExistingItem.CompareTo(objNewItem) == 0)
                            {
                                ++intTargetIndex;
                                objLoopExistingItem = objInnerLoopExistingItem;
                            }
                            else
                                break;
                        }

                        if (funcOverrideIfEquals != null)
                        {
                            funcOverrideIfEquals.Invoke(objLoopExistingItem, objNewItem);
                            return;
                        }

                        break;
                    }

                    if (intIntervalStart == intIntervalEnd)
                    {
                        if (intCompareResult > 0)
                            ++intTargetIndex;
                        break;
                    }

                    if (intCompareResult > 0)
                        intIntervalStart = intTargetIndex + 1;
                    else
                        intIntervalEnd = intTargetIndex - 1;
                }

                IDisposable objLocker2 = objHasLock?.LockObject.EnterWriteLock(token);
                try
                {
                    lstCollection.Insert(intTargetIndex, objNewItem);
                }
                finally
                {
                    objLocker2?.Dispose();
                }
            }
            finally
            {
                objLocker?.Dispose();
            }
        }

        public static void AddWithSort<T>(this IList<T> lstCollection, T objNewItem, IComparer<T> comparer,
            Action<T, T> funcOverrideIfEquals = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));
            IDisposable objLocker = null;
            if (lstCollection is IHasLockObject objHasLock)
                objLocker = objHasLock.LockObject.EnterUpgradeableReadLock(token);
            else
                objHasLock = null;
            try
            {
                token.ThrowIfCancellationRequested();
                // Binary search for the place where item should be inserted
                int intIntervalEnd = lstCollection.Count - 1;
                int intTargetIndex = intIntervalEnd / 2;
                for (int intIntervalStart = 0;
                     intIntervalStart <= intIntervalEnd;
                     intTargetIndex = (intIntervalStart + intIntervalEnd) / 2)
                {
                    token.ThrowIfCancellationRequested();
                    T objLoopExistingItem = lstCollection[intTargetIndex];
                    int intCompareResult = comparer.Compare(objLoopExistingItem, objNewItem);
                    if (intCompareResult == 0)
                    {
                        // Make sure we insert new items at the end of any equalities (so that order is maintained when adding multiple items)
                        for (int i = intTargetIndex + 1; i < lstCollection.Count; ++i)
                        {
                            T objInnerLoopExistingItem = lstCollection[i];
                            if (comparer.Compare(objInnerLoopExistingItem, objNewItem) == 0)
                            {
                                ++intTargetIndex;
                                objLoopExistingItem = objInnerLoopExistingItem;
                            }
                            else
                                break;
                        }

                        if (funcOverrideIfEquals != null)
                        {
                            funcOverrideIfEquals.Invoke(objLoopExistingItem, objNewItem);
                            return;
                        }

                        break;
                    }

                    if (intIntervalStart == intIntervalEnd)
                    {
                        if (intCompareResult > 0)
                            ++intTargetIndex;
                        break;
                    }

                    if (intCompareResult > 0)
                        intIntervalStart = intTargetIndex + 1;
                    else
                        intIntervalEnd = intTargetIndex - 1;
                }

                IDisposable objLocker2 = objHasLock?.LockObject.EnterWriteLock(token);
                try
                {
                    lstCollection.Insert(intTargetIndex, objNewItem);
                }
                finally
                {
                    objLocker2?.Dispose();
                }
            }
            finally
            {
                objLocker?.Dispose();
            }
        }

        public static void AddWithSort<T>(this IList<T> lstCollection, T objNewItem, Comparison<T> funcComparison,
            Action<T, T> funcOverrideIfEquals = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (funcComparison == null)
                throw new ArgumentNullException(nameof(funcComparison));
            IDisposable objLocker = null;
            if (lstCollection is IHasLockObject objHasLock)
                objLocker = objHasLock.LockObject.EnterUpgradeableReadLock(token);
            else
                objHasLock = null;
            try
            {
                token.ThrowIfCancellationRequested();
                // Binary search for the place where item should be inserted
                int intIntervalEnd = lstCollection.Count - 1;
                int intTargetIndex = intIntervalEnd / 2;
                for (int intIntervalStart = 0;
                     intIntervalStart <= intIntervalEnd;
                     intTargetIndex = (intIntervalStart + intIntervalEnd) / 2)
                {
                    token.ThrowIfCancellationRequested();
                    T objLoopExistingItem = lstCollection[intTargetIndex];
                    int intCompareResult = funcComparison.Invoke(objLoopExistingItem, objNewItem);
                    if (intCompareResult == 0)
                    {
                        // Make sure we insert new items at the end of any equalities (so that order is maintained when adding multiple items)
                        for (int i = intTargetIndex + 1; i < lstCollection.Count; ++i)
                        {
                            T objInnerLoopExistingItem = lstCollection[i];
                            if (funcComparison.Invoke(objInnerLoopExistingItem, objNewItem) == 0)
                            {
                                ++intTargetIndex;
                                objLoopExistingItem = objInnerLoopExistingItem;
                            }
                            else
                                break;
                        }

                        if (funcOverrideIfEquals != null)
                        {
                            funcOverrideIfEquals.Invoke(objLoopExistingItem, objNewItem);
                            return;
                        }

                        break;
                    }

                    if (intIntervalStart == intIntervalEnd)
                    {
                        if (intCompareResult > 0)
                            ++intTargetIndex;
                        break;
                    }

                    if (intCompareResult > 0)
                        intIntervalStart = intTargetIndex + 1;
                    else
                        intIntervalEnd = intTargetIndex - 1;
                }

                IDisposable objLocker2 = objHasLock?.LockObject.EnterWriteLock(token);
                try
                {
                    lstCollection.Insert(intTargetIndex, objNewItem);
                }
                finally
                {
                    objLocker2?.Dispose();
                }
            }
            finally
            {
                objLocker?.Dispose();
            }
        }

        public static void AddRangeWithSort<T>(this IList<T> lstCollection, IEnumerable<T> lstToAdd,
            Action<T, T> funcOverrideIfEquals = null, CancellationToken token = default) where T : IComparable
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            foreach (T objItem in lstToAdd)
                AddWithSort(lstCollection, objItem, funcOverrideIfEquals, token);
        }

        public static void AddRangeWithSort<T>(this IList<T> lstCollection, IEnumerable<T> lstToAdd,
            IComparer<T> comparer, Action<T, T> funcOverrideIfEquals = null, CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));
            foreach (T objItem in lstToAdd)
                AddWithSort(lstCollection, objItem, comparer, funcOverrideIfEquals, token);
        }

        public static void AddRangeWithSort<T>(this IList<T> lstCollection, IEnumerable<T> lstToAdd,
            Comparison<T> funcComparison, Action<T, T> funcOverrideIfEquals = null, CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            if (funcComparison == null)
                throw new ArgumentNullException(nameof(funcComparison));
            foreach (T objItem in lstToAdd)
                AddWithSort(lstCollection, objItem, funcComparison, funcOverrideIfEquals, token);
        }

        public static void RemoveRange<T>(this IList<T> lstCollection, int index, int count,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (count == 0)
                return;
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstCollection.Count == 0)
                return;
            if (index < 0 || index >= lstCollection.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            IDisposable objLocker = lstCollection is IHasLockObject objHasLockObject
                ? objHasLockObject.LockObject.EnterWriteLock(token)
                : null;
            try
            {
                for (int i = Math.Min(index + count - 1, lstCollection.Count); i >= index; --i)
                {
                    token.ThrowIfCancellationRequested();
                    lstCollection.RemoveAt(i);
                }
            }
            finally
            {
                objLocker?.Dispose();
            }
        }

        public static void RemoveAll<T>(this IList<T> lstCollection, Predicate<T> predicate,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            IDisposable objLocker = lstCollection is IHasLockObject objHasLockObject
                ? objHasLockObject.LockObject.EnterWriteLock(token)
                : null;
            try
            {
                for (int i = lstCollection.Count - 1; i >= 0; --i)
                {
                    token.ThrowIfCancellationRequested();
                    if (predicate(lstCollection[i]))
                    {
                        lstCollection.RemoveAt(i);
                    }
                }
            }
            finally
            {
                objLocker?.Dispose();
            }
        }

        public static void InsertRange<T>(this IList<T> lstCollection, int index, [NotNull] IEnumerable<T> collection,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            IDisposable objLocker = lstCollection is IHasLockObject objHasLockObject
                ? objHasLockObject.LockObject.EnterWriteLock(token)
                : null;
            try
            {
                foreach (T item in collection.Reverse())
                {
                    token.ThrowIfCancellationRequested();
                    lstCollection.Insert(index, item);
                }
            }
            finally
            {
                objLocker?.Dispose();
            }
        }

        public static Task SortAsync<T>(this IList<T> lstCollection, Func<T, T, Task<int>> comparer,
            CancellationToken token = default)
        {
            return lstCollection.SortAsync(0, lstCollection.Count, comparer, token);
        }

        public static async Task SortAsync<T>(this IList<T> lstCollection, int index, int length,
            Func<T, T, Task<int>> comparer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (length >= 2)
            {
                IAsyncDisposable objLocker = lstCollection is IHasLockObject objHasLockObject
                    ? await objHasLockObject.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false)
                    : null;
                try
                {
                    await IntroSortAsync(lstCollection, index, length + index - 1, 2 * lstCollection.Count.FloorLog2(),
                        comparer, token).ConfigureAwait(false);
                }
                finally
                {
                    if (objLocker != null)
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        private static async Task IntroSortAsync<T>(this IList<T> lstCollection, int lo, int hi, int depthLimit,
            Func<T, T, Task<int>> comparer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int num1;
            for (; hi > lo; hi = num1 - 1)
            {
                token.ThrowIfCancellationRequested();
                int num2 = hi - lo + 1;
                if (num2 <= 16)
                {
                    if (num2 == 1)
                        break;
                    if (num2 == 2)
                    {
                        await SwapIfGreaterAsync(lstCollection, comparer, lo, hi, token).ConfigureAwait(false);
                        break;
                    }

                    if (num2 == 3)
                    {
                        await SwapIfGreaterAsync(lstCollection, comparer, lo, hi - 1, token).ConfigureAwait(false);
                        await SwapIfGreaterAsync(lstCollection, comparer, lo, hi, token).ConfigureAwait(false);
                        await SwapIfGreaterAsync(lstCollection, comparer, hi - 1, hi, token).ConfigureAwait(false);
                        break;
                    }

                    await InsertionSortAsync(lstCollection, lo, hi, comparer, token).ConfigureAwait(false);
                    break;
                }

                if (depthLimit == 0)
                {
                    await HeapsortAsync(lstCollection, lo, hi, comparer, token).ConfigureAwait(false);
                    break;
                }

                --depthLimit;
                num1 = await PickPivotAndPartitionAsync(lstCollection, lo, hi, comparer, token).ConfigureAwait(false);
                await IntroSortAsync(lstCollection, num1 + 1, hi, depthLimit, comparer, token).ConfigureAwait(false);
            }
        }

        private static async Task SwapIfGreaterAsync<T>(this IList<T> lstCollection, Func<T, T, Task<int>> comparer,
            int a, int b, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (a != b && await comparer(lstCollection[a], lstCollection[b]).ConfigureAwait(false) > 0)
                (lstCollection[a], lstCollection[b]) = (lstCollection[b], lstCollection[a]);
        }

        private static Task SwapAsync<T>(this IList<T> lstCollection, int i, int j, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (i != j)
                (lstCollection[i], lstCollection[j]) = (lstCollection[j], lstCollection[i]);
            return Task.CompletedTask;
        }

        private static async Task<int> PickPivotAndPartitionAsync<T>(this IList<T> lstCollection, int lo, int hi,
            Func<T, T, Task<int>> comparer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int index = lo + (hi - lo) / 2;
            await SwapIfGreaterAsync(lstCollection, comparer, lo, index, token).ConfigureAwait(false);
            await SwapIfGreaterAsync(lstCollection, comparer, lo, hi, token).ConfigureAwait(false);
            await SwapIfGreaterAsync(lstCollection, comparer, index, hi, token).ConfigureAwait(false);
            T key = lstCollection[index];
            await SwapAsync(lstCollection, index, hi - 1, token).ConfigureAwait(false);
            int i = lo;
            int j = hi - 1;
            while (i < j)
            {
                do
                {
                    ++i;
                } while (await comparer(lstCollection[i], key).ConfigureAwait(false) < 0);

                do
                {
                    --j;
                } while (await comparer(key, lstCollection[j]).ConfigureAwait(false) < 0);

                if (i < j)
                    await SwapAsync(lstCollection, i, j, token).ConfigureAwait(false);
                else
                    break;
            }

            await SwapAsync(lstCollection, i, hi - 1, token).ConfigureAwait(false);
            return i;
        }

        private static async Task HeapsortAsync<T>(this IList<T> lstCollection, int lo, int hi,
            Func<T, T, Task<int>> comparer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int n = hi - lo + 1;
            for (int i = n / 2; i >= 1; --i)
                await DownHeapAsync(lstCollection, i, n, lo, comparer, token).ConfigureAwait(false);
            for (int index = n; index > 1; --index)
            {
                await SwapAsync(lstCollection, lo, lo + index - 1, token).ConfigureAwait(false);
                await DownHeapAsync(lstCollection, 1, index - 1, lo, comparer, token).ConfigureAwait(false);
            }
        }

        private static async Task DownHeapAsync<T>(this IList<T> lstCollection, int i, int n, int lo,
            Func<T, T, Task<int>> comparer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            T key = lstCollection[lo + i - 1];
            int num;
            for (; i <= n / 2; i = num)
            {
                token.ThrowIfCancellationRequested();
                num = 2 * i;
                if (num < n && await comparer(lstCollection[lo + num - 1], lstCollection[lo + num])
                        .ConfigureAwait(false) < 0)
                    ++num;
                if (await comparer(key, lstCollection[lo + num - 1]).ConfigureAwait(false) < 0)
                    lstCollection[lo + i - 1] = lstCollection[lo + num - 1];
                else
                    break;
            }

            lstCollection[lo + i - 1] = key;
        }

        private static async Task InsertionSortAsync<T>(this IList<T> lstCollection, int lo, int hi,
            Func<T, T, Task<int>> comparer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            for (int index1 = lo; index1 < hi; ++index1)
            {
                token.ThrowIfCancellationRequested();
                int index2 = index1;
                T key;
                for (key = lstCollection[index1 + 1];
                     index2 >= lo && await comparer(key, lstCollection[index2]).ConfigureAwait(false) < 0;
                     --index2)
                {
                    token.ThrowIfCancellationRequested();
                    lstCollection[index2 + 1] = lstCollection[index2];
                }

                lstCollection[index2 + 1] = key;
            }
        }

        /// <summary>
        /// Get a HashCode representing the contents of a collection in a way where the order of the items is irrelevant.
        /// Uses the parallel option for large enough collections where it could potentially be faster
        /// NOTE: GetEnsembleHashCode and GetOrderInvariantEnsembleHashCode will almost never be the same for the same collection!
        /// </summary>
        /// <typeparam name="T">The type for which GetHashCode() will be called</typeparam>
        /// <param name="lstItems">The collection containing the contents</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>A HashCode that is generated based on the contents of <paramref name="lstItems"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetOrderInvariantEnsembleHashCodeSmart<T>(this IList<T> lstItems, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return lstItems.Count > ushort.MaxValue
                ? lstItems.GetOrderInvariantEnsembleHashCodeParallel(token)
                : lstItems.GetOrderInvariantEnsembleHashCode(token);
        }

        /// <summary>
        /// Get a HashCode representing the contents of a collection in a way where the order of the items is irrelevant
        /// This is a parallelized version of GetOrderInvariantEnsembleHashCode meant to be used for large collections
        /// NOTE: GetEnsembleHashCode and GetOrderInvariantEnsembleHashCode will almost never be the same for the same collection!
        /// </summary>
        /// <typeparam name="T">The type for which GetHashCode() will be called</typeparam>
        /// <param name="lstItems">The collection containing the contents</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>A HashCode that is generated based on the contents of <paramref name="lstItems"/></returns>
        public static int GetOrderInvariantEnsembleHashCodeParallel<T>(this IList<T> lstItems, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstItems == null)
                return 0;
            // uint to prevent overflows
            unchecked
            {
                uint result = 0;
                Parallel.For(0, lstItems.Count, () => 0, (i, state, local) =>
                {
                    if (token.IsCancellationRequested)
                        state.Stop();
                    return state.IsStopped ? 0 : lstItems[i].GetHashCode();
                }, localResult => result += (uint)localResult);
                token.ThrowIfCancellationRequested();
                return (int)(19 + result * 31);
            }
        }
    }
}
