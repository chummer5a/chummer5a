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

        public static async Task AddWithSortAsync<T>(this IList<T> lstCollection, T objNewItem, Func<T, T, Task<int>> funcComparison,
            Action<T, T> funcOverrideIfEquals = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (funcComparison == null)
                throw new ArgumentNullException(nameof(funcComparison));
            IAsyncDisposable objLocker = null;
            if (lstCollection is IHasLockObject objHasLock)
                objLocker = await objHasLock.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
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
                    int intCompareResult = await funcComparison.Invoke(objLoopExistingItem, objNewItem).ConfigureAwait(false);
                    if (intCompareResult == 0)
                    {
                        // Make sure we insert new items at the end of any equalities (so that order is maintained when adding multiple items)
                        for (int i = intTargetIndex + 1; i < lstCollection.Count; ++i)
                        {
                            T objInnerLoopExistingItem = lstCollection[i];
                            if (await funcComparison.Invoke(objInnerLoopExistingItem, objNewItem).ConfigureAwait(false) == 0)
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

                IAsyncDisposable objLocker2 = objHasLock != null ? await objHasLock.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false) : null;
                try
                {
                    token.ThrowIfCancellationRequested();
                    lstCollection.Insert(intTargetIndex, objNewItem);
                }
                finally
                {
                    if (objLocker2 != null)
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                if (objLocker != null)
                    await objLocker.DisposeAsync().ConfigureAwait(false);
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

        public static async Task AddRangeWithSortAsync<T>(this IList<T> lstCollection, IEnumerable<T> lstToAdd,
            Func<T, T, Task<int>> funcComparison, Action<T, T> funcOverrideIfEquals = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            if (funcComparison == null)
                throw new ArgumentNullException(nameof(funcComparison));
            foreach (T objItem in lstToAdd)
                await AddWithSortAsync(lstCollection, objItem, funcComparison, funcOverrideIfEquals, token).ConfigureAwait(false);
        }

        /// <inheritdoc cref="List.RemoveRange(int, int)"/>
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
                if (lstCollection is List<T> lstCastCollection)
                {
                    lstCastCollection.RemoveRange(index, count);
                    return;
                }
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

        /// <inheritdoc cref="List.RemoveAll(Predicate{T})"/>
        public static int RemoveAll<T>(this IList<T> lstCollection, Predicate<T> predicate,
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
                if (lstCollection is List<T> lstCastCollection)
                    return lstCastCollection.RemoveAll(predicate);
                int intReturn = 0;
                for (int i = lstCollection.Count - 1; i >= 0; --i)
                {
                    token.ThrowIfCancellationRequested();
                    if (predicate(lstCollection[i]))
                    {
                        lstCollection.RemoveAt(i);
                        ++intReturn;
                    }
                }
                return intReturn;
            }
            finally
            {
                objLocker?.Dispose();
            }
        }

        public static async Task<int> RemoveAllAsync<T>(this IList<T> lstCollection, Func<T, Task<bool>> predicate,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            IAsyncDisposable objLocker = lstCollection is IHasLockObject objHasLockObject
                ? await objHasLockObject.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false)
                : null;
            try
            {
                int intReturn = 0;
                for (int i = lstCollection.Count - 1; i >= 0; --i)
                {
                    token.ThrowIfCancellationRequested();
                    if (await predicate(lstCollection[i]).ConfigureAwait(false))
                    {
                        lstCollection.RemoveAt(i);
                        ++intReturn;
                    }
                }
                return intReturn;
            }
            finally
            {
                if (objLocker != null)
                    await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List.InsertRange(int, IEnumerable{T})"/>
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

        /// <inheritdoc cref="List.Sort()"/>
        public static void Sort<T>(this IList<T> lstCollection)
        {
            if (lstCollection is List<T> lstCollectionCast)
                lstCollectionCast.Sort();
            else if (lstCollection is T[] lstArray)
                Array.Sort(lstArray);
            else
                lstCollection.Sort(0, lstCollection.Count, null);
        }

        /// <inheritdoc cref="List.Sort(IComparer{T})"/>
        public static void Sort<T>(this IList<T> lstCollection, IComparer<T> comparer)
        {
            if (lstCollection is List<T> lstCollectionCast)
                lstCollectionCast.Sort(comparer);
            else if (lstCollection is T[] lstArray)
                Array.Sort(lstArray, comparer);
            else
                lstCollection.Sort(0, lstCollection.Count, comparer);
        }

        /// <inheritdoc cref="List.Sort(int, int, IComparer{T})"/>
        public static void Sort<T>(this IList<T> lstCollection, int index, int count, IComparer<T> comparer)
        {
            if (lstCollection is List<T> lstCollectionCast)
            {
                lstCollectionCast.Sort(index, count, comparer);
                return;
            }
            if (lstCollection is T[] lstArray)
            {
                Array.Sort(lstArray, index, count, comparer);
                return;
            }

            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (count == 0)
                return;

            if (lstCollection.Count - index < count)
                throw new ArgumentOutOfRangeException(nameof(count));

            IntrospectiveSort(lstCollection, index, count, comparer);
        }

        /// <inheritdoc cref="List.Sort(Comparison{T})"/>
        public static void Sort<T>(this IList<T> lstCollection, Comparison<T> comparison)
        {
            if (lstCollection is List<T> lstCollectionCast)
            {
                lstCollectionCast.Sort(comparison);
                return;
            }
            if (lstCollection is T[] lstArray)
            {
                Array.Sort(lstArray, comparison);
                return;
            }

            if (comparison == null)
                throw new ArgumentNullException(nameof(comparison));

            int intCount = lstCollection.Count;
            if (intCount > 0)
            {
                IComparer<T> comparer = new FunctorComparer<T>(comparison);
                IntrospectiveSort(lstCollection, 0, intCount, comparer);
            }
        }

        private readonly struct FunctorComparer<T> : IComparer<T>
        {
            private readonly Comparison<T> comparison;

            public FunctorComparer(Comparison<T> comparison)
            {
                this.comparison = comparison;
            }

            public int Compare(T x, T y)
            {
                return comparison(x, y);
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

        private static void IntrospectiveSort<T>(IList<T> keys, int left, int length, IComparer<T> comparer)
        {
            if (length >= 2)
            {
                IntroSort(keys, left, length + left - 1, 2 * keys.Count.FloorLog2(), comparer);
            }
        }

        private static void IntroSort<T>(IList<T> keys, int lo, int hi, int depthLimit, IComparer<T> comparer)
        {
            while (hi > lo)
            {
                int num = hi - lo + 1;
                if (num <= 16)
                {
                    switch (num)
                    {
                        case 1:
                            break;
                        case 2:
                            SwapIfGreater(keys, comparer, lo, hi);
                            break;
                        case 3:
                            SwapIfGreater(keys, comparer, lo, hi - 1);
                            SwapIfGreater(keys, comparer, lo, hi);
                            SwapIfGreater(keys, comparer, hi - 1, hi);
                            break;
                        default:
                            InsertionSort(keys, lo, hi, comparer);
                            break;
                    }

                    break;
                }

                if (depthLimit == 0)
                {
                    Heapsort(keys, lo, hi, comparer);
                    break;
                }

                depthLimit--;
                int num2 = PickPivotAndPartition(keys, lo, hi, comparer);
                IntroSort(keys, num2 + 1, hi, depthLimit, comparer);
                hi = num2 - 1;
            }
        }

        private static void SwapIfGreater<T>(IList<T> keys, IComparer<T> comparer, int a, int b)
        {
            if (a != b && comparer.Compare(keys[a], keys[b]) > 0)
            {
                (keys[b], keys[a]) = (keys[a], keys[b]);
            }
        }

        private static void Swap<T>(IList<T> a, int i, int j)
        {
            if (i != j)
            {
                (a[j], a[i]) = (a[i], a[j]);
            }
        }

        private static int PickPivotAndPartition<T>(IList<T> keys, int lo, int hi, IComparer<T> comparer)
        {
            int num = lo + (hi - lo) / 2;
            SwapIfGreater(keys, comparer, lo, num);
            SwapIfGreater(keys, comparer, lo, hi);
            SwapIfGreater(keys, comparer, num, hi);
            T val = keys[num];
            Swap(keys, num, hi - 1);
            int num2 = lo;
            int num3 = hi - 1;
            while (num2 < num3)
            {
                do
                {
                    ++num2;
                }
                while (comparer.Compare(keys[num2], val) < 0);

                do
                {
                    --num3;
                }
                while (comparer.Compare(val, keys[num3]) < 0);

                if (num2 < num3)
                    Swap(keys, num2, num3);
            }

            Swap(keys, num2, hi - 1);
            return num2;
        }

        private static void Heapsort<T>(IList<T> keys, int lo, int hi, IComparer<T> comparer)
        {
            int num = hi - lo + 1;
            for (int num2 = num / 2; num2 >= 1; --num2)
            {
                DownHeap(keys, num2, num, lo, comparer);
            }

            for (int num3 = num; num3 > 1; --num3)
            {
                Swap(keys, lo, lo + num3 - 1);
                DownHeap(keys, 1, num3 - 1, lo, comparer);
            }
        }

        private static void DownHeap<T>(IList<T> keys, int i, int n, int lo, IComparer<T> comparer)
        {
            T val = keys[lo + i - 1];
            int num;
            for (; i <= n / 2; i = num)
            {
                num = 2 * i;
                int right = lo + num - 1;
                if (num < n && comparer.Compare(keys[right], keys[lo + num]) < 0)
                    right++;
                if (comparer.Compare(val, keys[right]) < 0)
                    keys[lo + i - 1] = keys[right];
                else
                    break;
            }

            keys[lo + i - 1] = val;
        }

        private static void InsertionSort<T>(IList<T> keys, int lo, int hi, IComparer<T> comparer)
        {
            for (int i = lo; i < hi; i++)
            {
                int num = i;
                T val = keys[i + 1];
                for (; num >= lo && comparer.Compare(val, keys[num]) < 0; --num)
                {
                    keys[num + 1] = keys[num];
                }

                keys[num + 1] = val;
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
                int right = lo + num - 1;
                if (num < n && await comparer(lstCollection[right], lstCollection[lo + num])
                        .ConfigureAwait(false) < 0)
                    ++right;
                if (await comparer(key, lstCollection[right]).ConfigureAwait(false) < 0)
                    lstCollection[lo + i - 1] = lstCollection[right];
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
    }
}
