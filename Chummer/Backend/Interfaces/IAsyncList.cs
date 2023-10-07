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
    public interface IAsyncList<T> : IList<T>, IAsyncCollection<T>
    {
        ValueTask<T> GetValueAtAsync(int index, CancellationToken token = default);

        ValueTask SetValueAtAsync(int index, T value, CancellationToken token = default);

        ValueTask<int> IndexOfAsync(T item, CancellationToken token = default);

        ValueTask InsertAsync(int index, T item, CancellationToken token = default);

        ValueTask RemoveAtAsync(int index, CancellationToken token = default);
    }

    public static class AsyncListExtensions
    {
        public static async Task AddWithSortAsync<T>(this IAsyncList<T> lstCollection, T objNewItem,
                                                     Action<T, T> funcOverrideIfEquals = null,
                                                     CancellationToken token = default) where T : IComparable
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstCollection is IHasLockObject objLocker)
                await objLocker.LockObject.SimpleEnterReadLockAsync(token).ConfigureAwait(false);
            else
                objLocker = null;
            try
            {
                // Binary search for the place where item should be inserted
                int intIntervalEnd = await lstCollection.GetCountAsync(token).ConfigureAwait(false) - 1;
                int intTargetIndex = intIntervalEnd / 2;
                for (int intIntervalStart = 0;
                     intIntervalStart <= intIntervalEnd;
                     intTargetIndex = (intIntervalStart + intIntervalEnd) / 2)
                {
                    T objLoopExistingItem
                        = await lstCollection.GetValueAtAsync(intTargetIndex, token).ConfigureAwait(false);
                    int intCompareResult = objLoopExistingItem.CompareTo(objNewItem);
                    if (intCompareResult == 0)
                    {
                        // Make sure we insert new items at the end of any equalities (so that order is maintained when adding multiple items)
                        for (int i = intTargetIndex + 1;
                             i < await lstCollection.GetCountAsync(token).ConfigureAwait(false);
                             ++i)
                        {
                            T objInnerLoopExistingItem
                                = await lstCollection.GetValueAtAsync(i, token).ConfigureAwait(false);
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

                IAsyncDisposable objLocker2 = objLocker != null
                    ? await objLocker.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false)
                    : null;
                try
                {
                    await lstCollection.InsertAsync(intTargetIndex, objNewItem, token).ConfigureAwait(false);
                }
                finally
                {
                    if (objLocker2 != null)
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                objLocker?.LockObject.ExitReadLock();
            }
        }

        public static async Task AddWithSortAsync<T>(this IAsyncList<T> lstCollection, T objNewItem, IComparer<T> comparer,
                                                     Action<T, T> funcOverrideIfEquals = null,
                                                     CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));
            if (lstCollection is IHasLockObject objLocker)
                await objLocker.LockObject.SimpleEnterReadLockAsync(token).ConfigureAwait(false);
            else
                objLocker = null;
            try
            {
                // Binary search for the place where item should be inserted
                int intIntervalEnd = await lstCollection.GetCountAsync(token).ConfigureAwait(false) - 1;
                int intTargetIndex = intIntervalEnd / 2;
                for (int intIntervalStart = 0;
                     intIntervalStart <= intIntervalEnd;
                     intTargetIndex = (intIntervalStart + intIntervalEnd) / 2)
                {
                    T objLoopExistingItem
                        = await lstCollection.GetValueAtAsync(intTargetIndex, token).ConfigureAwait(false);
                    int intCompareResult = comparer.Compare(objLoopExistingItem, objNewItem);
                    if (intCompareResult == 0)
                    {
                        // Make sure we insert new items at the end of any equalities (so that order is maintained when adding multiple items)
                        for (int i = intTargetIndex + 1;
                             i < await lstCollection.GetCountAsync(token).ConfigureAwait(false);
                             ++i)
                        {
                            T objInnerLoopExistingItem
                                = await lstCollection.GetValueAtAsync(i, token).ConfigureAwait(false);
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

                IAsyncDisposable objLocker2 = objLocker != null
                    ? await objLocker.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false)
                    : null;
                try
                {
                    await lstCollection.InsertAsync(intTargetIndex, objNewItem, token).ConfigureAwait(false);
                }
                finally
                {
                    if (objLocker2 != null)
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                objLocker?.LockObject.ExitReadLock();
            }
        }

        public static async Task AddWithSortAsync<T>(this IAsyncList<T> lstCollection, T objNewItem,
                                                     Comparison<T> funcComparison, Action<T, T> funcOverrideIfEquals = null,
                                                     CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (funcComparison == null)
                throw new ArgumentNullException(nameof(funcComparison));
            if (lstCollection is IHasLockObject objLocker)
                await objLocker.LockObject.SimpleEnterReadLockAsync(token).ConfigureAwait(false);
            else
                objLocker = null;
            try
            {
                // Binary search for the place where item should be inserted
                int intIntervalEnd = await lstCollection.GetCountAsync(token).ConfigureAwait(false) - 1;
                int intTargetIndex = intIntervalEnd / 2;
                for (int intIntervalStart = 0;
                     intIntervalStart <= intIntervalEnd;
                     intTargetIndex = (intIntervalStart + intIntervalEnd) / 2)
                {
                    T objLoopExistingItem
                        = await lstCollection.GetValueAtAsync(intTargetIndex, token).ConfigureAwait(false);
                    int intCompareResult = funcComparison.Invoke(objLoopExistingItem, objNewItem);
                    if (intCompareResult == 0)
                    {
                        // Make sure we insert new items at the end of any equalities (so that order is maintained when adding multiple items)
                        for (int i = intTargetIndex + 1;
                             i < await lstCollection.GetCountAsync(token).ConfigureAwait(false);
                             ++i)
                        {
                            T objInnerLoopExistingItem
                                = await lstCollection.GetValueAtAsync(i, token).ConfigureAwait(false);
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

                IAsyncDisposable objLocker2 = objLocker != null
                    ? await objLocker.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false)
                    : null;
                try
                {
                    await lstCollection.InsertAsync(intTargetIndex, objNewItem, token).ConfigureAwait(false);
                }
                finally
                {
                    if (objLocker2 != null)
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                objLocker?.LockObject.ExitReadLock();
            }
        }

        public static async Task AddRangeWithSortAsync<T>(this IAsyncList<T> lstCollection, IEnumerable<T> lstToAdd,
                                                          Action<T, T> funcOverrideIfEquals = null,
                                                          CancellationToken token = default) where T : IComparable
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            foreach (T objItem in lstToAdd)
                await AddWithSortAsync(lstCollection, objItem, funcOverrideIfEquals, token).ConfigureAwait(false);
        }

        public static async Task AddRangeWithSortAsync<T>(this IAsyncList<T> lstCollection, IEnumerable<T> lstToAdd,
                                                          IComparer<T> comparer, Action<T, T> funcOverrideIfEquals = null,
                                                          CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));
            foreach (T objItem in lstToAdd)
                await AddWithSortAsync(lstCollection, objItem, comparer, funcOverrideIfEquals, token).ConfigureAwait(false);
        }

        public static async Task AddRangeWithSortAsync<T>(this IAsyncList<T> lstCollection, IEnumerable<T> lstToAdd,
                                                          Comparison<T> funcComparison,
                                                          Action<T, T> funcOverrideIfEquals = null,
                                                          CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            if (funcComparison == null)
                throw new ArgumentNullException(nameof(funcComparison));
            foreach (T objItem in lstToAdd)
                await AddWithSortAsync(lstCollection, objItem, funcComparison, funcOverrideIfEquals, token).ConfigureAwait(false);
        }

        public static async Task AddAsyncRangeWithSortAsync<T>(this IAsyncList<T> lstCollection, IAsyncEnumerable<T> lstToAdd,
                                                          Action<T, T> funcOverrideIfEquals = null,
                                                          CancellationToken token = default) where T : IComparable
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            using (IEnumerator<T> objEnumerator = await lstToAdd.GetEnumeratorAsync(token).ConfigureAwait(false))
            {
                while (objEnumerator.MoveNext())
                {
                    await AddWithSortAsync(lstCollection, objEnumerator.Current, funcOverrideIfEquals, token).ConfigureAwait(false);
                }
            }
        }

        public static async Task AddAsyncRangeWithSortAsync<T>(this IAsyncList<T> lstCollection, IAsyncEnumerable<T> lstToAdd,
                                                          IComparer<T> comparer, Action<T, T> funcOverrideIfEquals = null,
                                                          CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));
            using (IEnumerator<T> objEnumerator = await lstToAdd.GetEnumeratorAsync(token).ConfigureAwait(false))
            {
                while (objEnumerator.MoveNext())
                {
                    await AddWithSortAsync(lstCollection, objEnumerator.Current, comparer, funcOverrideIfEquals, token).ConfigureAwait(false);
                }
            }
        }

        public static async Task AddAsyncRangeWithSortAsync<T>(this IAsyncList<T> lstCollection, IAsyncEnumerable<T> lstToAdd,
                                                          Comparison<T> funcComparison,
                                                          Action<T, T> funcOverrideIfEquals = null,
                                                          CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            if (funcComparison == null)
                throw new ArgumentNullException(nameof(funcComparison));
            using (IEnumerator<T> objEnumerator = await lstToAdd.GetEnumeratorAsync(token).ConfigureAwait(false))
            {
                while (objEnumerator.MoveNext())
                {
                    await AddWithSortAsync(lstCollection, objEnumerator.Current, funcComparison, funcOverrideIfEquals, token).ConfigureAwait(false);
                }
            }
        }

        public static async Task AddWithSortAsync<T>(this IAsyncList<T> lstCollection, T objNewItem,
                                                     Func<T, T, Task> funcOverrideIfEquals,
                                                     CancellationToken token = default) where T : IComparable
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstCollection is IHasLockObject objLocker)
                await objLocker.LockObject.SimpleEnterReadLockAsync(token).ConfigureAwait(false);
            else
                objLocker = null;
            try
            {
                // Binary search for the place where item should be inserted
                int intIntervalEnd = await lstCollection.GetCountAsync(token).ConfigureAwait(false) - 1;
                int intTargetIndex = intIntervalEnd / 2;
                for (int intIntervalStart = 0;
                     intIntervalStart <= intIntervalEnd;
                     intTargetIndex = (intIntervalStart + intIntervalEnd) / 2)
                {
                    T objLoopExistingItem
                        = await lstCollection.GetValueAtAsync(intTargetIndex, token).ConfigureAwait(false);
                    int intCompareResult = objLoopExistingItem.CompareTo(objNewItem);
                    if (intCompareResult == 0)
                    {
                        // Make sure we insert new items at the end of any equalities (so that order is maintained when adding multiple items)
                        for (int i = intTargetIndex + 1;
                             i < await lstCollection.GetCountAsync(token).ConfigureAwait(false);
                             ++i)
                        {
                            T objInnerLoopExistingItem
                                = await lstCollection.GetValueAtAsync(i, token).ConfigureAwait(false);
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
                            await funcOverrideIfEquals.Invoke(objLoopExistingItem, objNewItem).ConfigureAwait(false);
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

                IAsyncDisposable objLocker2 = objLocker != null
                    ? await objLocker.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false)
                    : null;
                try
                {
                    await lstCollection.InsertAsync(intTargetIndex, objNewItem, token).ConfigureAwait(false);
                }
                finally
                {
                    if (objLocker2 != null)
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                objLocker?.LockObject.ExitReadLock();
            }
        }

        public static async Task AddWithSortAsync<T>(this IAsyncList<T> lstCollection, T objNewItem, IComparer<T> comparer,
                                                     Func<T, T, Task> funcOverrideIfEquals,
                                                     CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));
            if (lstCollection is IHasLockObject objLocker)
                await objLocker.LockObject.SimpleEnterReadLockAsync(token).ConfigureAwait(false);
            else
                objLocker = null;
            try
            {
                // Binary search for the place where item should be inserted
                int intIntervalEnd = await lstCollection.GetCountAsync(token).ConfigureAwait(false) - 1;
                int intTargetIndex = intIntervalEnd / 2;
                for (int intIntervalStart = 0;
                     intIntervalStart <= intIntervalEnd;
                     intTargetIndex = (intIntervalStart + intIntervalEnd) / 2)
                {
                    T objLoopExistingItem
                        = await lstCollection.GetValueAtAsync(intTargetIndex, token).ConfigureAwait(false);
                    int intCompareResult = comparer.Compare(objLoopExistingItem, objNewItem);
                    if (intCompareResult == 0)
                    {
                        // Make sure we insert new items at the end of any equalities (so that order is maintained when adding multiple items)
                        for (int i = intTargetIndex + 1;
                             i < await lstCollection.GetCountAsync(token).ConfigureAwait(false);
                             ++i)
                        {
                            T objInnerLoopExistingItem
                                = await lstCollection.GetValueAtAsync(i, token).ConfigureAwait(false);
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
                            await funcOverrideIfEquals.Invoke(objLoopExistingItem, objNewItem).ConfigureAwait(false);
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

                IAsyncDisposable objLocker2 = objLocker != null
                    ? await objLocker.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false)
                    : null;
                try
                {
                    await lstCollection.InsertAsync(intTargetIndex, objNewItem, token).ConfigureAwait(false);
                }
                finally
                {
                    if (objLocker2 != null)
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                objLocker?.LockObject.ExitReadLock();
            }
        }

        public static async Task AddWithSortAsync<T>(this IAsyncList<T> lstCollection, T objNewItem,
                                                     Comparison<T> funcComparison, Func<T, T, Task> funcOverrideIfEquals,
                                                     CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (funcComparison == null)
                throw new ArgumentNullException(nameof(funcComparison));
            if (lstCollection is IHasLockObject objLocker)
                await objLocker.LockObject.SimpleEnterReadLockAsync(token).ConfigureAwait(false);
            else
                objLocker = null;
            try
            {
                // Binary search for the place where item should be inserted
                int intIntervalEnd = await lstCollection.GetCountAsync(token).ConfigureAwait(false) - 1;
                int intTargetIndex = intIntervalEnd / 2;
                for (int intIntervalStart = 0;
                     intIntervalStart <= intIntervalEnd;
                     intTargetIndex = (intIntervalStart + intIntervalEnd) / 2)
                {
                    T objLoopExistingItem
                        = await lstCollection.GetValueAtAsync(intTargetIndex, token).ConfigureAwait(false);
                    int intCompareResult = funcComparison.Invoke(objLoopExistingItem, objNewItem);
                    if (intCompareResult == 0)
                    {
                        // Make sure we insert new items at the end of any equalities (so that order is maintained when adding multiple items)
                        for (int i = intTargetIndex + 1;
                             i < await lstCollection.GetCountAsync(token).ConfigureAwait(false);
                             ++i)
                        {
                            T objInnerLoopExistingItem
                                = await lstCollection.GetValueAtAsync(i, token).ConfigureAwait(false);
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
                            await funcOverrideIfEquals.Invoke(objLoopExistingItem, objNewItem).ConfigureAwait(false);
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

                IAsyncDisposable objLocker2 = objLocker != null
                    ? await objLocker.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false)
                    : null;
                try
                {
                    await lstCollection.InsertAsync(intTargetIndex, objNewItem, token).ConfigureAwait(false);
                }
                finally
                {
                    if (objLocker2 != null)
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                objLocker?.LockObject.ExitReadLock();
            }
        }

        public static async Task AddWithSortAsync<T>(this IAsyncList<T> lstCollection, T objNewItem,
                                                     Func<T, T, Task<int>> funcComparison, Action<T, T> funcOverrideIfEquals = null,
                                                     CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (funcComparison == null)
                throw new ArgumentNullException(nameof(funcComparison));
            if (lstCollection is IHasLockObject objLocker)
                await objLocker.LockObject.SimpleEnterReadLockAsync(token).ConfigureAwait(false);
            else
                objLocker = null;
            try
            {
                // Binary search for the place where item should be inserted
                int intIntervalEnd = await lstCollection.GetCountAsync(token).ConfigureAwait(false) - 1;
                int intTargetIndex = intIntervalEnd / 2;
                for (int intIntervalStart = 0;
                     intIntervalStart <= intIntervalEnd;
                     intTargetIndex = (intIntervalStart + intIntervalEnd) / 2)
                {
                    T objLoopExistingItem
                        = await lstCollection.GetValueAtAsync(intTargetIndex, token).ConfigureAwait(false);
                    int intCompareResult = await funcComparison(objLoopExistingItem, objNewItem).ConfigureAwait(false);
                    if (intCompareResult == 0)
                    {
                        // Make sure we insert new items at the end of any equalities (so that order is maintained when adding multiple items)
                        for (int i = intTargetIndex + 1;
                             i < await lstCollection.GetCountAsync(token).ConfigureAwait(false);
                             ++i)
                        {
                            T objInnerLoopExistingItem
                                = await lstCollection.GetValueAtAsync(i, token).ConfigureAwait(false);
                            if (await funcComparison(objInnerLoopExistingItem, objNewItem).ConfigureAwait(false) == 0)
                            {
                                ++intTargetIndex;
                                objLoopExistingItem = objInnerLoopExistingItem;
                            }
                            else
                                break;
                        }

                        if (funcOverrideIfEquals != null)
                        {
                            funcOverrideIfEquals(objLoopExistingItem, objNewItem);
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

                IAsyncDisposable objLocker2 = objLocker != null
                    ? await objLocker.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false)
                    : null;
                try
                {
                    await lstCollection.InsertAsync(intTargetIndex, objNewItem, token).ConfigureAwait(false);
                }
                finally
                {
                    if (objLocker2 != null)
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                objLocker?.LockObject.ExitReadLock();
            }
        }

        public static async Task AddWithSortAsync<T>(this IAsyncList<T> lstCollection, T objNewItem,
                                                     Func<T, T, Task<int>> funcComparison, Func<T, T, Task> funcOverrideIfEquals,
                                                     CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (funcComparison == null)
                throw new ArgumentNullException(nameof(funcComparison));
            if (lstCollection is IHasLockObject objLocker)
                await objLocker.LockObject.SimpleEnterReadLockAsync(token).ConfigureAwait(false);
            else
                objLocker = null;
            try
            {
                // Binary search for the place where item should be inserted
                int intIntervalEnd = await lstCollection.GetCountAsync(token).ConfigureAwait(false) - 1;
                int intTargetIndex = intIntervalEnd / 2;
                for (int intIntervalStart = 0;
                     intIntervalStart <= intIntervalEnd;
                     intTargetIndex = (intIntervalStart + intIntervalEnd) / 2)
                {
                    T objLoopExistingItem
                        = await lstCollection.GetValueAtAsync(intTargetIndex, token).ConfigureAwait(false);
                    int intCompareResult = await funcComparison(objLoopExistingItem, objNewItem).ConfigureAwait(false);
                    if (intCompareResult == 0)
                    {
                        // Make sure we insert new items at the end of any equalities (so that order is maintained when adding multiple items)
                        for (int i = intTargetIndex + 1;
                             i < await lstCollection.GetCountAsync(token).ConfigureAwait(false);
                             ++i)
                        {
                            T objInnerLoopExistingItem
                                = await lstCollection.GetValueAtAsync(i, token).ConfigureAwait(false);
                            if (await funcComparison(objInnerLoopExistingItem, objNewItem).ConfigureAwait(false) == 0)
                            {
                                ++intTargetIndex;
                                objLoopExistingItem = objInnerLoopExistingItem;
                            }
                            else
                                break;
                        }

                        if (funcOverrideIfEquals != null)
                        {
                            await funcOverrideIfEquals.Invoke(objLoopExistingItem, objNewItem).ConfigureAwait(false);
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

                IAsyncDisposable objLocker2 = objLocker != null
                    ? await objLocker.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false)
                    : null;
                try
                {
                    await lstCollection.InsertAsync(intTargetIndex, objNewItem, token).ConfigureAwait(false);
                }
                finally
                {
                    if (objLocker2 != null)
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                objLocker?.LockObject.ExitReadLock();
            }
        }

        public static async Task AddRangeWithSortAsync<T>(this IAsyncList<T> lstCollection, IEnumerable<T> lstToAdd,
                                                          Func<T, T, Task> funcOverrideIfEquals,
                                                          CancellationToken token = default) where T : IComparable
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            foreach (T objItem in lstToAdd)
                await AddWithSortAsync(lstCollection, objItem, funcOverrideIfEquals, token).ConfigureAwait(false);
        }

        public static async Task AddRangeWithSortAsync<T>(this IAsyncList<T> lstCollection, IEnumerable<T> lstToAdd,
                                                          IComparer<T> comparer, Func<T, T, Task> funcOverrideIfEquals,
                                                          CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));
            foreach (T objItem in lstToAdd)
                await AddWithSortAsync(lstCollection, objItem, comparer, funcOverrideIfEquals, token).ConfigureAwait(false);
        }

        public static async Task AddRangeWithSortAsync<T>(this IAsyncList<T> lstCollection, IEnumerable<T> lstToAdd,
                                                          Comparison<T> funcComparison,
                                                          Func<T, T, Task> funcOverrideIfEquals,
                                                          CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            if (funcComparison == null)
                throw new ArgumentNullException(nameof(funcComparison));
            foreach (T objItem in lstToAdd)
                await AddWithSortAsync(lstCollection, objItem, funcComparison, funcOverrideIfEquals, token).ConfigureAwait(false);
        }

        public static async Task AddRangeWithSortAsync<T>(this IAsyncList<T> lstCollection, IEnumerable<T> lstToAdd,
                                                          Func<T, T, Task<int>> funcComparison,
                                                          Action<T, T> funcOverrideIfEquals = null,
                                                          CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            if (funcComparison == null)
                throw new ArgumentNullException(nameof(funcComparison));
            foreach (T objItem in lstToAdd)
                await AddWithSortAsync(lstCollection, objItem, funcComparison, funcOverrideIfEquals, token).ConfigureAwait(false);
        }

        public static async Task AddRangeWithSortAsync<T>(this IAsyncList<T> lstCollection, IEnumerable<T> lstToAdd,
                                                          Func<T, T, Task<int>> funcComparison,
                                                          Func<T, T, Task> funcOverrideIfEquals,
                                                          CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            if (funcComparison == null)
                throw new ArgumentNullException(nameof(funcComparison));
            foreach (T objItem in lstToAdd)
                await AddWithSortAsync(lstCollection, objItem, funcComparison, funcOverrideIfEquals, token).ConfigureAwait(false);
        }

        public static async Task AddAsyncRangeWithSortAsync<T>(this IAsyncList<T> lstCollection, IAsyncEnumerable<T> lstToAdd,
                                                          Func<T, T, Task> funcOverrideIfEquals,
                                                          CancellationToken token = default) where T : IComparable
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            using (IEnumerator<T> objEnumerator = await lstToAdd.GetEnumeratorAsync(token).ConfigureAwait(false))
            {
                while (objEnumerator.MoveNext())
                {
                    await AddWithSortAsync(lstCollection, objEnumerator.Current, funcOverrideIfEquals, token).ConfigureAwait(false);
                }
            }
        }

        public static async Task AddAsyncRangeWithSortAsync<T>(this IAsyncList<T> lstCollection, IAsyncEnumerable<T> lstToAdd,
                                                          IComparer<T> comparer, Func<T, T, Task> funcOverrideIfEquals,
                                                          CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));
            using (IEnumerator<T> objEnumerator = await lstToAdd.GetEnumeratorAsync(token).ConfigureAwait(false))
            {
                while (objEnumerator.MoveNext())
                {
                    await AddWithSortAsync(lstCollection, objEnumerator.Current, comparer, funcOverrideIfEquals, token).ConfigureAwait(false);
                }
            }
        }

        public static async Task AddAsyncRangeWithSortAsync<T>(this IAsyncList<T> lstCollection, IAsyncEnumerable<T> lstToAdd,
                                                          Comparison<T> funcComparison,
                                                          Func<T, T, Task> funcOverrideIfEquals,
                                                          CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            if (funcComparison == null)
                throw new ArgumentNullException(nameof(funcComparison));
            using (IEnumerator<T> objEnumerator = await lstToAdd.GetEnumeratorAsync(token).ConfigureAwait(false))
            {
                while (objEnumerator.MoveNext())
                {
                    await AddWithSortAsync(lstCollection, objEnumerator.Current, funcComparison, funcOverrideIfEquals, token).ConfigureAwait(false);
                }
            }
        }

        public static async Task AddAsyncRangeWithSortAsync<T>(this IAsyncList<T> lstCollection, IAsyncEnumerable<T> lstToAdd,
                                                               Func<T, T, Task<int>> funcComparison,
                                                               Action<T, T> funcOverrideIfEquals = null,
                                                               CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            if (funcComparison == null)
                throw new ArgumentNullException(nameof(funcComparison));
            using (IEnumerator<T> objEnumerator = await lstToAdd.GetEnumeratorAsync(token).ConfigureAwait(false))
            {
                while (objEnumerator.MoveNext())
                {
                    await AddWithSortAsync(lstCollection, objEnumerator.Current, funcComparison, funcOverrideIfEquals, token).ConfigureAwait(false);
                }
            }
        }

        public static async Task AddAsyncRangeWithSortAsync<T>(this IAsyncList<T> lstCollection, IAsyncEnumerable<T> lstToAdd,
                                                               Func<T, T, Task<int>> funcComparison,
                                                               Func<T, T, Task> funcOverrideIfEquals,
                                                               CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (lstToAdd == null)
                throw new ArgumentNullException(nameof(lstToAdd));
            if (funcComparison == null)
                throw new ArgumentNullException(nameof(funcComparison));
            using (IEnumerator<T> objEnumerator = await lstToAdd.GetEnumeratorAsync(token).ConfigureAwait(false))
            {
                while (objEnumerator.MoveNext())
                {
                    await AddWithSortAsync(lstCollection, objEnumerator.Current, funcComparison, funcOverrideIfEquals, token).ConfigureAwait(false);
                }
            }
        }

        public static async Task RemoveRangeAsync<T>(this IAsyncList<T> lstCollection, int index, int count,
                                                     CancellationToken token = default)
        {
            if (count == 0)
                return;
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (await lstCollection.GetCountAsync(token).ConfigureAwait(false) == 0)
                return;
            if (index < 0 || index >= await lstCollection.GetCountAsync(token).ConfigureAwait(false))
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            IAsyncDisposable objLocker = null;
            if (lstCollection is IHasLockObject objHasLockObject)
                objLocker = await objHasLockObject.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                for (int i = Math.Min(index + count - 1,
                                      await lstCollection.GetCountAsync(token).ConfigureAwait(false));
                     i >= index;
                     --i)
                    await lstCollection.RemoveAtAsync(i, token).ConfigureAwait(false);
            }
            finally
            {
                if (objLocker != null)
                    await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public static async Task RemoveAllAsync<T>(this IAsyncList<T> lstCollection, Predicate<T> predicate,
                                                   CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            IAsyncDisposable objLocker = null;
            if (lstCollection is IHasLockObject objHasLockObject)
                objLocker = await objHasLockObject.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                for (int i = await lstCollection.GetCountAsync(token).ConfigureAwait(false) - 1; i >= 0; --i)
                {
                    if (predicate(await lstCollection.GetValueAtAsync(i, token).ConfigureAwait(false)))
                    {
                        await lstCollection.RemoveAtAsync(i, token).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                if (objLocker != null)
                    await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public static async Task RemoveAllAsync<T>(this IAsyncList<T> lstCollection, Func<T, Task<bool>> predicate,
                                              CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            IAsyncDisposable objLocker = null;
            if (lstCollection is IHasLockObject objHasLockObject)
                objLocker = await objHasLockObject.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                for (int i = await lstCollection.GetCountAsync(token).ConfigureAwait(false) - 1; i >= 0; --i)
                {
                    if (await predicate.Invoke(await lstCollection.GetValueAtAsync(i, token).ConfigureAwait(false))
                                       .ConfigureAwait(false))
                    {
                        await lstCollection.RemoveAtAsync(i, token).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                if (objLocker != null)
                    await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public static async Task InsertRangeAsync<T>(this IAsyncList<T> lstCollection, int index,
                                                     [NotNull] IEnumerable<T> collection, CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            IAsyncDisposable objLocker = null;
            if (lstCollection is IHasLockObject objHasLockObject)
                objLocker = await objHasLockObject.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                foreach (T item in collection.Reverse())
                {
                    await lstCollection.InsertAsync(index, item, token).ConfigureAwait(false);
                }
            }
            finally
            {
                if (objLocker != null)
                    await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public static async Task InsertAsyncRangeAsync<T>(this IAsyncList<T> lstCollection, int index,
                                                     [NotNull] IAsyncEnumerable<T> collection, CancellationToken token = default)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            List<T> lstToAdd = await collection.ToListAsync(token).ConfigureAwait(false);
            lstToAdd.Reverse();
            IAsyncDisposable objLocker = null;
            if (lstCollection is IHasLockObject objHasLockObject)
                objLocker = await objHasLockObject.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                foreach (T item in lstToAdd)
                {
                    await lstCollection.InsertAsync(index, item, token).ConfigureAwait(false);
                }
            }
            finally
            {
                if (objLocker != null)
                    await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
