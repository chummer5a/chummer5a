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
        Task<T> GetValueAtAsync(int index, CancellationToken token = default);

        Task SetValueAtAsync(int index, T value, CancellationToken token = default);

        Task<int> IndexOfAsync(T item, CancellationToken token = default);

        Task InsertAsync(int index, T item, CancellationToken token = default);

        Task RemoveAtAsync(int index, CancellationToken token = default);
    }

    public static class AsyncListExtensions
    {
        public static async Task AddWithSortAsync<T>(this IAsyncList<T> lstCollection, T objNewItem,
                                                     Action<T, T> funcOverrideIfEquals = null,
                                                     CancellationToken token = default) where T : IComparable
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            IAsyncDisposable objLocker = null;
            if (lstCollection is IHasLockObject objHasLock)
                objLocker = await objHasLock.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            else
                objHasLock = null;
            try
            {
                token.ThrowIfCancellationRequested();
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

                IAsyncDisposable objLocker2 = objHasLock != null
                    ? await objHasLock.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false)
                    : null;
                try
                {
                    token.ThrowIfCancellationRequested();
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
                if (objLocker != null)
                    await objLocker.DisposeAsync().ConfigureAwait(false);
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
            IAsyncDisposable objLocker = null;
            if (lstCollection is IHasLockObject objHasLock)
                objLocker = await objHasLock.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            else
                objHasLock = null;
            try
            {
                token.ThrowIfCancellationRequested();
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

                IAsyncDisposable objLocker2 = objHasLock != null
                    ? await objHasLock.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false)
                    : null;
                try
                {
                    token.ThrowIfCancellationRequested();
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
                if (objLocker != null)
                    await objLocker.DisposeAsync().ConfigureAwait(false);
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
            IAsyncDisposable objLocker = null;
            if (lstCollection is IHasLockObject objHasLock)
                objLocker = await objHasLock.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            else
                objHasLock = null;
            try
            {
                token.ThrowIfCancellationRequested();
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

                IAsyncDisposable objLocker2 = objHasLock != null
                    ? await objHasLock.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false)
                    : null;
                try
                {
                    token.ThrowIfCancellationRequested();
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
                if (objLocker != null)
                    await objLocker.DisposeAsync().ConfigureAwait(false);
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
            IAsyncDisposable objLocker = null;
            if (lstCollection is IHasLockObject objHasLock)
                objLocker = await objHasLock.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            else
                objHasLock = null;
            try
            {
                token.ThrowIfCancellationRequested();
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

                IAsyncDisposable objLocker2 = objHasLock != null
                    ? await objHasLock.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false)
                    : null;
                try
                {
                    token.ThrowIfCancellationRequested();
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
                if (objLocker != null)
                    await objLocker.DisposeAsync().ConfigureAwait(false);
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
            IAsyncDisposable objLocker = null;
            if (lstCollection is IHasLockObject objHasLock)
                objLocker = await objHasLock.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            else
                objHasLock = null;
            try
            {
                token.ThrowIfCancellationRequested();
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

                IAsyncDisposable objLocker2 = objHasLock != null
                    ? await objHasLock.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false)
                    : null;
                try
                {
                    token.ThrowIfCancellationRequested();
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
                if (objLocker != null)
                    await objLocker.DisposeAsync().ConfigureAwait(false);
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
            IAsyncDisposable objLocker = null;
            if (lstCollection is IHasLockObject objHasLock)
                objLocker = await objHasLock.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            else
                objHasLock = null;
            try
            {
                token.ThrowIfCancellationRequested();
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

                IAsyncDisposable objLocker2 = objHasLock != null
                    ? await objHasLock.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false)
                    : null;
                try
                {
                    token.ThrowIfCancellationRequested();
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
                if (objLocker != null)
                    await objLocker.DisposeAsync().ConfigureAwait(false);
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
            IAsyncDisposable objLocker = null;
            if (lstCollection is IHasLockObject objHasLock)
                objLocker = await objHasLock.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            else
                objHasLock = null;
            try
            {
                token.ThrowIfCancellationRequested();
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

                IAsyncDisposable objLocker2 = objHasLock != null
                    ? await objHasLock.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false)
                    : null;
                try
                {
                    token.ThrowIfCancellationRequested();
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
                if (objLocker != null)
                    await objLocker.DisposeAsync().ConfigureAwait(false);
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
            IAsyncDisposable objLocker = null;
            if (lstCollection is IHasLockObject objHasLock)
                objLocker = await objHasLock.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            else
                objHasLock = null;
            try
            {
                token.ThrowIfCancellationRequested();
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

                IAsyncDisposable objLocker2 = objHasLock != null
                    ? await objHasLock.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false)
                    : null;
                try
                {
                    token.ThrowIfCancellationRequested();
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
                if (objLocker != null)
                    await objLocker.DisposeAsync().ConfigureAwait(false);
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
                token.ThrowIfCancellationRequested();
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
                token.ThrowIfCancellationRequested();
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
                token.ThrowIfCancellationRequested();
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
                token.ThrowIfCancellationRequested();
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
                token.ThrowIfCancellationRequested();
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

        public static Task SortAsync<T>(this IAsyncList<T> lstCollection, Func<T, T, Task<int>> comparer,
            CancellationToken token = default)
        {
            return lstCollection.SortAsync(0, lstCollection.Count, comparer, token);
        }

        public static async Task SortAsync<T>(this IAsyncList<T> lstCollection, int index, int length,
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

        private static async Task IntroSortAsync<T>(this IAsyncList<T> lstCollection, int lo, int hi, int depthLimit,
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

        private static async Task SwapIfGreaterAsync<T>(this IAsyncList<T> lstCollection, Func<T, T, Task<int>> comparer,
            int a, int b, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (a != b)
            {
                T lhs = await lstCollection.GetValueAtAsync(a, token).ConfigureAwait(false);
                T rhs = await lstCollection.GetValueAtAsync(b, token).ConfigureAwait(false);
                if (await comparer(lhs, rhs).ConfigureAwait(false) > 0)
                {
                    await lstCollection.SetValueAtAsync(a, rhs, token).ConfigureAwait(false);
                    await lstCollection.SetValueAtAsync(b, lhs, token).ConfigureAwait(false);
                }
            }
        }

        private static async Task SwapAsync<T>(this IAsyncList<T> lstCollection, int i, int j, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (i != j)
            {
                T lhs = await lstCollection.GetValueAtAsync(i, token).ConfigureAwait(false);
                T rhs = await lstCollection.GetValueAtAsync(j, token).ConfigureAwait(false);
                await lstCollection.SetValueAtAsync(i, rhs, token).ConfigureAwait(false);
                await lstCollection.SetValueAtAsync(j, lhs, token).ConfigureAwait(false);
            }
        }

        private static async Task<int> PickPivotAndPartitionAsync<T>(this IAsyncList<T> lstCollection, int lo, int hi,
            Func<T, T, Task<int>> comparer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int index = lo + (hi - lo) / 2;
            await SwapIfGreaterAsync(lstCollection, comparer, lo, index, token).ConfigureAwait(false);
            await SwapIfGreaterAsync(lstCollection, comparer, lo, hi, token).ConfigureAwait(false);
            await SwapIfGreaterAsync(lstCollection, comparer, index, hi, token).ConfigureAwait(false);
            T key = await lstCollection.GetValueAtAsync(index, token).ConfigureAwait(false);
            await SwapAsync(lstCollection, index, hi - 1, token).ConfigureAwait(false);
            int i = lo;
            int j = hi - 1;
            while (i < j)
            {
                do
                {
                    ++i;
                } while (await comparer(await lstCollection.GetValueAtAsync(i, token).ConfigureAwait(false), key)
                             .ConfigureAwait(false) < 0);

                do
                {
                    --j;
                } while (await comparer(key, await lstCollection.GetValueAtAsync(j, token).ConfigureAwait(false))
                             .ConfigureAwait(false) < 0);

                if (i < j)
                    await SwapAsync(lstCollection, i, j, token).ConfigureAwait(false);
                else
                    break;
            }

            await SwapAsync(lstCollection, i, hi - 1, token).ConfigureAwait(false);
            return i;
        }

        private static async Task HeapsortAsync<T>(this IAsyncList<T> lstCollection, int lo, int hi,
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

        private static async Task DownHeapAsync<T>(this IAsyncList<T> lstCollection, int i, int n, int lo,
            Func<T, T, Task<int>> comparer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            T key = await lstCollection.GetValueAtAsync(lo + i - 1, token).ConfigureAwait(false);
            int num;
            for (; i <= n / 2; i = num)
            {
                token.ThrowIfCancellationRequested();
                num = 2 * i;
                T key2 = await lstCollection.GetValueAtAsync(lo + num - 1, token).ConfigureAwait(false);
                if (num < n)
                {
                    T key3 = await lstCollection.GetValueAtAsync(lo + num, token).ConfigureAwait(false);
                    if (await comparer(key2, key3).ConfigureAwait(false) < 0)
                        key2 = key3;
                }

                if (await comparer(key, key2).ConfigureAwait(false) < 0)
                    await lstCollection.SetValueAtAsync(lo + i - 1, key2, token).ConfigureAwait(false);
                else
                    break;
            }

            await lstCollection.SetValueAtAsync(lo + i - 1, key, token).ConfigureAwait(false);
        }

        private static async Task InsertionSortAsync<T>(this IAsyncList<T> lstCollection, int lo, int hi,
            Func<T, T, Task<int>> comparer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            for (int index1 = lo; index1 < hi; ++index1)
            {
                token.ThrowIfCancellationRequested();
                int index2 = index1;
                T key = await lstCollection.GetValueAtAsync(index1 + 1, token).ConfigureAwait(false);
                if (index2 >= lo)
                {
                    for (T key2 = await lstCollection.GetValueAtAsync(index2, token).ConfigureAwait(false);
                         await comparer(key, key2).ConfigureAwait(false) < 0;
                         key2 = await lstCollection.GetValueAtAsync(index2, token).ConfigureAwait(false))
                    {
                        token.ThrowIfCancellationRequested();
                        await lstCollection.SetValueAtAsync(index2 + 1, key2, token)
                            .ConfigureAwait(false);
                        if (--index2 < lo)
                            break;
                    }
                }

                await lstCollection.SetValueAtAsync(index2 + 1, key, token).ConfigureAwait(false);
            }
        }
    }
}
