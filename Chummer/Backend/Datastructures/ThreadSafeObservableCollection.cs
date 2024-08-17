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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    public class ThreadSafeObservableCollection<T> : IAsyncList<T>, IList, IAsyncReadOnlyList<T>, INotifyCollectionChanged, IAsyncProducerConsumerCollection<T>, IAsyncEnumerableWithSideEffects<T>, IHasLockObject
    {
        [CLSCompliant(false)]
        protected readonly EnhancedObservableCollection<T> _lstData;

        public AsyncFriendlyReaderWriterLock LockObject { get; }

        public ThreadSafeObservableCollection(AsyncFriendlyReaderWriterLock objParentLock = null, bool blnLockReadOnlyForParent = false)
        {
            LockObject = new AsyncFriendlyReaderWriterLock(objParentLock, blnLockReadOnlyForParent);
            _lstData = new EnhancedObservableCollection<T>
            {
                CollectionChangedLock = LockObject
            };
        }

        public ThreadSafeObservableCollection(IEnumerable<T> collection, AsyncFriendlyReaderWriterLock objParentLock = null, bool blnLockReadOnlyForParent = false)
        {
            LockObject = new AsyncFriendlyReaderWriterLock(objParentLock, blnLockReadOnlyForParent);
            _lstData = new EnhancedObservableCollection<T>(collection)
            {
                CollectionChangedLock = LockObject
            };
        }

        public ThreadSafeObservableCollection(List<T> list, AsyncFriendlyReaderWriterLock objParentLock = null, bool blnLockReadOnlyForParent = false)
        {
            LockObject = new AsyncFriendlyReaderWriterLock(objParentLock, blnLockReadOnlyForParent);
            _lstData = new EnhancedObservableCollection<T>(list)
            {
                CollectionChangedLock = LockObject
            };
        }

        /// <inheritdoc cref="ObservableCollection{T}.Count" />
        public int Count
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstData.Count;
            }
        }

        /// <inheritdoc />
        public object SyncRoot => LockObject;

        /// <inheritdoc />
        public bool IsSynchronized => true;

        public bool IsReadOnly => false;

        /// <inheritdoc />
        public bool IsFixedSize => false;

        public async Task<int> GetCountAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await _lstData.GetCountAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public void Remove(object value)
        {
            Remove((T)value);
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            using (LockObject.EnterWriteLock())
                return _lstData.Remove(item);
        }

        /// <inheritdoc cref="List{T}.Remove(T)" />
        public async Task<bool> RemoveAsync(T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await _lstData.RemoveAsync(item, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.RemoveAt" />
        public void RemoveAt(int index)
        {
            using (LockObject.EnterWriteLock())
                _lstData.RemoveAt(index);
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.RemoveAt" />
        public async Task RemoveAtAsync(int index, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                await _lstData.RemoveAtAsync(index, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        object IList.this[int index]
        {
            get => this[index];
            set => this[index] = (T)value;
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}" />
        public T this[int index]
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstData[index];
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_lstData[index].Equals(value))
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_lstData[index].Equals(value))
                        return;
                    using (LockObject.EnterWriteLock())
                        _lstData[index] = value;
                }
            }
        }

        public async Task<T> GetValueAtAsync(int index, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await _lstData.GetValueAtAsync(index, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetValueAtAsync(int index, T value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if ((await _lstData.GetValueAtAsync(index, token).ConfigureAwait(false)).Equals(value))
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if ((await _lstData.GetValueAtAsync(index, token).ConfigureAwait(false)).Equals(value))
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    await _lstData.SetValueAtAsync(index, value, token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public virtual int Add(object value)
        {
            if (!(value is T objCastValue))
                return -1;
            using (LockObject.EnterWriteLock())
            {
                _lstData.Add(objCastValue);
                return _lstData.Count - 1;
            }
        }

        /// <inheritdoc />
        public virtual void Add(T item)
        {
            using (LockObject.EnterWriteLock())
                _lstData.Add(item);
        }

        public virtual async Task AddAsync(T item, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                await _lstData.AddAsync(item, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public virtual bool TryAdd(T item)
        {
            Add(item);
            return true;
        }

        public void Clear()
        {
            using (LockObject.EnterWriteLock())
                _lstData.Clear();
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.Clear" />
        public async Task ClearAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                await _lstData.ClearAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public void Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.Insert" />
        public virtual void Insert(int index, T item)
        {
            using (LockObject.EnterWriteLock())
                _lstData.Insert(index, item);
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.Insert" />
        public virtual async Task InsertAsync(int index, T item, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                await _lstData.InsertAsync(index, item, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.Move" />
        public void Move(int oldIndex, int newIndex)
        {
            using (LockObject.EnterWriteLock())
                _lstData.Move(oldIndex, newIndex);
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.Move" />
        public async Task MoveAsync(int oldIndex, int newIndex, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                await _lstData.MoveAsync(oldIndex, newIndex, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool Contains(object value)
        {
            return Contains((T)value);
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.Contains" />
        public bool Contains(T item)
        {
            using (LockObject.EnterReadLock())
                return _lstData.Contains(item);
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.Contains" />
        public async Task<bool> ContainsAsync(T item, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await _lstData.ContainsAsync(item, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public void CopyTo(Array array, int index)
        {
            using (LockObject.EnterReadLock())
            {
                foreach (T objItem in _lstData)
                {
                    array.SetValue(objItem, index);
                    ++index;
                }
            }
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.CopyTo(T[],int)" />
        public void CopyTo(T[] array, int arrayIndex)
        {
            using (LockObject.EnterReadLock())
                _lstData.CopyTo(array, arrayIndex);
        }

        public async Task CopyToAsync(T[] array, int index, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                await _lstData.CopyToAsync(array, index, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public virtual async Task<bool> TryAddAsync(T item, CancellationToken token = default)
        {
            await AddAsync(item, token).ConfigureAwait(false);
            return true;
        }

        /// <inheritdoc />
        public bool TryTake(out T item)
        {
            using (LockObject.EnterReadLock())
            {
                if (_lstData.Count == 0)
                {
                    item = default;
                    return false;
                }
            }

            using (LockObject.EnterUpgradeableReadLock())
            {
                if (_lstData.Count > 0)
                {
                    // FIFO to be compliant with how the default for BlockingCollection<T> is ConcurrentQueue
                    item = _lstData[0];
                    using (LockObject.EnterWriteLock())
                        _lstData.RemoveAt(0);
                    return true;
                }
            }

            item = default;
            return false;
        }

        public async Task<Tuple<bool, T>> TryTakeAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (await _lstData.GetCountAsync(token).ConfigureAwait(false) == 0)
                    return new Tuple<bool, T>(false, default);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (await _lstData.GetCountAsync(token).ConfigureAwait(false) > 0)
                {
                    // FIFO to be compliant with how the default for BlockingCollection<T> is ConcurrentQueue
                    T objReturn = await _lstData.GetValueAtAsync(0, token).ConfigureAwait(false);
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        await _lstData.RemoveAtAsync(0, token).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }

                    return new Tuple<bool, T>(true, objReturn);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            return new Tuple<bool, T>(false, default);
        }

        /// <inheritdoc />
        public T[] ToArray()
        {
            using (LockObject.EnterReadLock())
            {
                T[] aobjReturn = new T[Count];
                for (int i = 0; i < Count; ++i)
                {
                    aobjReturn[i] = _lstData[i];
                }

                return aobjReturn;
            }
        }

        public async Task<T[]> ToArrayAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                T[] aobjReturn = new T[await GetCountAsync(token).ConfigureAwait(false)];
                token.ThrowIfCancellationRequested();
                for (int i = 0; i < await GetCountAsync(token).ConfigureAwait(false); ++i)
                    aobjReturn[i] = await _lstData.GetValueAtAsync(i, token).ConfigureAwait(false);
                return aobjReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            LockingEnumerator<T> objReturn = LockingEnumerator<T>.Get(this);
            objReturn.SetEnumerator(_lstData.GetEnumerator());
            return objReturn;
        }

        public Task<IEnumerator<T>> GetEnumeratorAsync(CancellationToken token = default)
        {
            // Needs to be like this (using async inner function) to make sure AsyncLocals for parents are set in proper location
            Task<LockingEnumerator<T>> tskReturn = LockingEnumerator<T>.GetAsync(this, token);
            return Inner(tskReturn);
            async Task<IEnumerator<T>> Inner(Task<LockingEnumerator<T>> tskInner)
            {
                LockingEnumerator<T> objResult = await tskInner.ConfigureAwait(false);
                objResult.SetEnumerator(_lstData.GetEnumerator());
                return objResult;
            }
        }

        public IEnumerator<T> EnumerateWithSideEffects()
        {
            LockingEnumerator<T> objReturn = LockingEnumerator<T>.GetWithSideEffects(this);
            objReturn.SetEnumerator(_lstData.GetEnumerator());
            return objReturn;
        }

        public Task<IEnumerator<T>> EnumerateWithSideEffectsAsync(CancellationToken token = default)
        {
            // Needs to be like this (using async inner function) to make sure AsyncLocals for parents are set in proper location
            Task<LockingEnumerator<T>> tskReturn = LockingEnumerator<T>.GetWithSideEffectsAsync(this, token);
            return Inner(tskReturn);
            async Task<IEnumerator<T>> Inner(Task<LockingEnumerator<T>> tskInner)
            {
                LockingEnumerator<T> objResult = await tskInner.ConfigureAwait(false);
                objResult.SetEnumerator(_lstData.GetEnumerator());
                return objResult;
            }
        }

        public int IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.IndexOf(T)" />
        public int IndexOf(T item)
        {
            using (LockObject.EnterReadLock())
                return _lstData.IndexOf(item);
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.IndexOf(T)" />
        public async Task<int> IndexOfAsync(T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await _lstData.IndexOfAsync(item, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.Sort()" />
        public void Sort()
        {
            using (LockObject.EnterReadLock())
            {
                if (_lstData.Count == 0)
                    return;
            }

            using (LockObject.EnterUpgradeableReadLock())
            {
                if (_lstData.Count == 0)
                    return;
                IDisposable[] aobjLockers = _lstData[0] is IHasLockObject ? new IDisposable[_lstData.Count] : null;
                T[] aobjSorted = new T[_lstData.Count];
                for (int i = 0; i < _lstData.Count; ++i)
                {
                    T objLoop = _lstData[i];
                    aobjSorted[i] = objLoop;
                    if (aobjLockers != null)
                        aobjLockers[i] = (objLoop as IHasLockObject)?.LockObject.EnterReadLock();
                }

                Array.Sort(aobjSorted);

                if (aobjLockers != null)
                {
                    foreach (IDisposable objLocker in aobjLockers)
                    {
                        objLocker.Dispose();
                    }
                }

                using (LockObject.EnterWriteLock())
                {
                    for (int i = 0; i < aobjSorted.Length; ++i)
                        _lstData.Move(_lstData.IndexOf(aobjSorted[i]), i);
                }
            }
        }

        /// <inheritdoc cref="List{T}.Sort(IComparer{T})" />
        public void Sort(IComparer<T> comparer)
        {
            using (LockObject.EnterReadLock())
            {
                if (_lstData.Count == 0)
                    return;
            }

            using (LockObject.EnterUpgradeableReadLock())
            {
                if (_lstData.Count == 0)
                    return;
                IDisposable[] aobjLockers = _lstData[0] is IHasLockObject ? new IDisposable[_lstData.Count] : null;
                T[] aobjSorted = new T[_lstData.Count];
                for (int i = 0; i < _lstData.Count; ++i)
                {
                    T objLoop = _lstData[i];
                    aobjSorted[i] = objLoop;
                    if (aobjLockers != null)
                        aobjLockers[i] = (objLoop as IHasLockObject)?.LockObject.EnterReadLock();
                }

                Array.Sort(aobjSorted, comparer);

                if (aobjLockers != null)
                {
                    foreach (IDisposable objLocker in aobjLockers)
                    {
                        objLocker.Dispose();
                    }
                }

                using (LockObject.EnterWriteLock())
                {
                    for (int i = 0; i < aobjSorted.Length; ++i)
                        _lstData.Move(_lstData.IndexOf(aobjSorted[i]), i);
                }
            }
        }

        /// <inheritdoc cref="List{T}.Sort(int, int, IComparer{T})" />
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            if (count == 0)
                return;
            using (LockObject.EnterReadLock())
            {
                if (_lstData.Count == 0)
                    return;
            }

            using (LockObject.EnterUpgradeableReadLock())
            {
                if (_lstData.Count == 0)
                    return;
                IDisposable[] aobjLockers = _lstData[0] is IHasLockObject ? new IDisposable[count] : null;
                T[] aobjSorted = new T[_lstData.Count];
                for (int i = 0; i < count; ++i)
                {
                    T objLoop = _lstData[index + i];
                    aobjSorted[i] = objLoop;
                    if (aobjLockers != null)
                        aobjLockers[i] = (objLoop as IHasLockObject)?.LockObject.EnterReadLock();
                }

                Array.Sort(aobjSorted, comparer);

                if (aobjLockers != null)
                {
                    foreach (IDisposable objLocker in aobjLockers)
                    {
                        objLocker.Dispose();
                    }
                }

                using (LockObject.EnterWriteLock())
                {
                    for (int i = 0; i < aobjSorted.Length; ++i)
                        _lstData.Move(_lstData.IndexOf(aobjSorted[i]), index + i);
                }
            }
        }

        /// <inheritdoc cref="List{T}.Sort(Comparison{T})" />
        public void Sort(Comparison<T> comparison)
        {
            using (LockObject.EnterReadLock())
            {
                if (_lstData.Count == 0)
                    return;
            }

            using (LockObject.EnterUpgradeableReadLock())
            {
                if (_lstData.Count == 0)
                    return;
                IDisposable[] aobjLockers = _lstData[0] is IHasLockObject ? new IDisposable[_lstData.Count] : null;
                T[] aobjSorted = new T[_lstData.Count];
                for (int i = 0; i < _lstData.Count; ++i)
                {
                    T objLoop = _lstData[i];
                    aobjSorted[i] = objLoop;
                    if (aobjLockers != null)
                        aobjLockers[i] = (objLoop as IHasLockObject)?.LockObject.EnterReadLock();
                }

                Array.Sort(aobjSorted, comparison);

                if (aobjLockers != null)
                {
                    foreach (IDisposable objLocker in aobjLockers)
                    {
                        objLocker.Dispose();
                    }
                }

                using (LockObject.EnterWriteLock())
                {
                    for (int i = 0; i < aobjSorted.Length; ++i)
                        _lstData.Move(_lstData.IndexOf(aobjSorted[i]), i);
                }
            }
        }

        /// <inheritdoc cref="List{T}.Sort()" />
        public async Task SortAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_lstData.Count == 0)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intCount = await _lstData.GetCountAsync(token).ConfigureAwait(false);
                if (intCount == 0)
                    return;
                Stack<IAsyncDisposable> stkLockers =
                    await _lstData.GetValueAtAsync(0, token).ConfigureAwait(false) is IHasLockObject
                        ? new Stack<IAsyncDisposable>(intCount)
                        : null;
                T[] aobjSorted = new T[intCount];
                token.ThrowIfCancellationRequested();
                try
                {
                    for (int i = 0; i < intCount; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objLoop = await _lstData.GetValueAtAsync(i, token).ConfigureAwait(false);
                        aobjSorted[i] = objLoop;
                        if (stkLockers != null && objLoop is IHasLockObject objLoopWithLocker)
                            stkLockers.Push(await objLoopWithLocker.LockObject.EnterReadLockAsync(token)
                                .ConfigureAwait(false));
                    }

                    token.ThrowIfCancellationRequested();
                    Array.Sort(aobjSorted);
                }
                finally
                {
                    if (stkLockers != null)
                    {
                        while (stkLockers.Count > 0)
                        {
                            IAsyncDisposable objLocker3 = stkLockers.Pop();
                            if (objLocker3 != null)
                                await objLocker3.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }

                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    for (int i = 0; i < aobjSorted.Length; ++i)
                        await _lstData.MoveAsync(await _lstData.IndexOfAsync(aobjSorted[i], token).ConfigureAwait(false), i, token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.Sort(IComparer{T})" />
        public async Task SortAsync(IComparer<T> comparer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_lstData.Count == 0)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intCount = await _lstData.GetCountAsync(token).ConfigureAwait(false);
                if (intCount == 0)
                    return;
                Stack<IAsyncDisposable> stkLockers =
                    await _lstData.GetValueAtAsync(0, token).ConfigureAwait(false) is IHasLockObject
                        ? new Stack<IAsyncDisposable>(intCount)
                        : null;
                T[] aobjSorted = new T[intCount];
                token.ThrowIfCancellationRequested();
                try
                {
                    for (int i = 0; i < intCount; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objLoop = await _lstData.GetValueAtAsync(i, token).ConfigureAwait(false);
                        aobjSorted[i] = objLoop;
                        if (stkLockers != null && objLoop is IHasLockObject objLoopWithLocker)
                            stkLockers.Push(await objLoopWithLocker.LockObject.EnterReadLockAsync(token)
                                .ConfigureAwait(false));
                    }

                    token.ThrowIfCancellationRequested();
                    Array.Sort(aobjSorted, comparer);
                }
                finally
                {
                    if (stkLockers != null)
                    {
                        while (stkLockers.Count > 0)
                        {
                            IAsyncDisposable objLocker3 = stkLockers.Pop();
                            if (objLocker3 != null)
                                await objLocker3.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }

                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    for (int i = 0; i < aobjSorted.Length; ++i)
                        await _lstData.MoveAsync(await _lstData.IndexOfAsync(aobjSorted[i], token).ConfigureAwait(false), i, token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.Sort(int, int, IComparer{T})" />
        public async Task SortAsync(int index, int count, IComparer<T> comparer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (count == 0)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_lstData.Count == 0)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intCount = await _lstData.GetCountAsync(token).ConfigureAwait(false);
                if (intCount == 0)
                    return;
                Stack<IAsyncDisposable> stkLockers =
                    await _lstData.GetValueAtAsync(0, token).ConfigureAwait(false) is IHasLockObject
                        ? new Stack<IAsyncDisposable>(intCount)
                        : null;
                T[] aobjSorted = new T[intCount];
                token.ThrowIfCancellationRequested();
                try
                {
                    for (int i = 0; i < count; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objLoop = await _lstData.GetValueAtAsync(index + i, token).ConfigureAwait(false);
                        aobjSorted[i] = objLoop;
                        if (stkLockers != null && objLoop is IHasLockObject objLoopWithLocker)
                            stkLockers.Push(await objLoopWithLocker.LockObject.EnterReadLockAsync(token)
                                .ConfigureAwait(false));
                    }

                    token.ThrowIfCancellationRequested();
                    Array.Sort(aobjSorted, comparer);
                }
                finally
                {
                    if (stkLockers != null)
                    {
                        while (stkLockers.Count > 0)
                        {
                            IAsyncDisposable objLocker3 = stkLockers.Pop();
                            if (objLocker3 != null)
                                await objLocker3.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }

                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    for (int i = 0; i < aobjSorted.Length; ++i)
                        await _lstData.MoveAsync(await _lstData.IndexOfAsync(aobjSorted[i], token).ConfigureAwait(false), i, token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.Sort(Comparison{T})" />
        public async Task SortAsync(Comparison<T> comparison, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_lstData.Count == 0)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intCount = await _lstData.GetCountAsync(token).ConfigureAwait(false);
                if (intCount == 0)
                    return;
                Stack<IAsyncDisposable> stkLockers =
                    await _lstData.GetValueAtAsync(0, token).ConfigureAwait(false) is IHasLockObject
                        ? new Stack<IAsyncDisposable>(intCount)
                        : null;
                T[] aobjSorted = new T[intCount];
                token.ThrowIfCancellationRequested();
                try
                {
                    for (int i = 0; i < intCount; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objLoop = await _lstData.GetValueAtAsync(i, token).ConfigureAwait(false);
                        aobjSorted[i] = objLoop;
                        if (stkLockers != null && objLoop is IHasLockObject objLoopWithLocker)
                            stkLockers.Push(await objLoopWithLocker.LockObject.EnterReadLockAsync(token)
                                .ConfigureAwait(false));
                    }

                    token.ThrowIfCancellationRequested();
                    Array.Sort(aobjSorted, comparison);
                }
                finally
                {
                    if (stkLockers != null)
                    {
                        while (stkLockers.Count > 0)
                        {
                            IAsyncDisposable objLocker3 = stkLockers.Pop();
                            if (objLocker3 != null)
                                await objLocker3.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }

                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    for (int i = 0; i < aobjSorted.Length; ++i)
                        await _lstData.MoveAsync(await _lstData.IndexOfAsync(aobjSorted[i], token).ConfigureAwait(false), i, token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SortAsync(Func<T, T, Task<int>> comparer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_lstData.Count == 0)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intCount = await _lstData.GetCountAsync(token).ConfigureAwait(false);
                if (intCount == 0)
                    return;
                Stack<IAsyncDisposable> stkLockers =
                    await _lstData.GetValueAtAsync(0, token).ConfigureAwait(false) is IHasLockObject
                        ? new Stack<IAsyncDisposable>(intCount)
                        : null;
                T[] aobjSorted = new T[intCount];
                token.ThrowIfCancellationRequested();
                try
                {
                    for (int i = 0; i < intCount; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objLoop = await _lstData.GetValueAtAsync(i, token).ConfigureAwait(false);
                        aobjSorted[i] = objLoop;
                        if (stkLockers != null && objLoop is IHasLockObject objLoopWithLocker)
                            stkLockers.Push(await objLoopWithLocker.LockObject.EnterReadLockAsync(token)
                                .ConfigureAwait(false));
                    }

                    token.ThrowIfCancellationRequested();
                    await aobjSorted.SortAsync(comparer, token: token).ConfigureAwait(false);
                }
                finally
                {
                    if (stkLockers != null)
                    {
                        while (stkLockers.Count > 0)
                        {
                            IAsyncDisposable objLocker3 = stkLockers.Pop();
                            if (objLocker3 != null)
                                await objLocker3.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }

                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    for (int i = 0; i < aobjSorted.Length; ++i)
                        await _lstData.MoveAsync(await _lstData.IndexOfAsync(aobjSorted[i], token).ConfigureAwait(false), i, token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SortAsync(int index, int count, Func<T, T, Task<int>> comparer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (count == 0)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_lstData.Count == 0)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intCount = await _lstData.GetCountAsync(token).ConfigureAwait(false);
                if (intCount == 0)
                    return;
                Stack<IAsyncDisposable> stkLockers =
                    await _lstData.GetValueAtAsync(0, token).ConfigureAwait(false) is IHasLockObject
                        ? new Stack<IAsyncDisposable>(intCount)
                        : null;
                T[] aobjSorted = new T[intCount];
                token.ThrowIfCancellationRequested();
                try
                {
                    for (int i = 0; i < count; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objLoop = await _lstData.GetValueAtAsync(index + i, token).ConfigureAwait(false);
                        aobjSorted[i] = objLoop;
                        if (stkLockers != null && objLoop is IHasLockObject objLoopWithLocker)
                            stkLockers.Push(await objLoopWithLocker.LockObject.EnterReadLockAsync(token)
                                .ConfigureAwait(false));
                    }

                    token.ThrowIfCancellationRequested();
                    await aobjSorted.SortAsync(comparer, token: token).ConfigureAwait(false);
                }
                finally
                {
                    if (stkLockers != null)
                    {
                        while (stkLockers.Count > 0)
                        {
                            IAsyncDisposable objLocker3 = stkLockers.Pop();
                            if (objLocker3 != null)
                                await objLocker3.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }

                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    for (int i = 0; i < aobjSorted.Length; ++i)
                        await _lstData.MoveAsync(await _lstData.IndexOfAsync(aobjSorted[i], token).ConfigureAwait(false), i, token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public virtual event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                using (LockObject.EnterWriteLock())
                    _lstData.CollectionChanged += value;
            }
            remove
            {
                using (LockObject.EnterWriteLock())
                    _lstData.CollectionChanged -= value;
            }
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.BeforeClearCollectionChanged" />
        public virtual event NotifyCollectionChangedEventHandler BeforeClearCollectionChanged
        {
            add
            {
                using (LockObject.EnterWriteLock())
                    _lstData.BeforeClearCollectionChanged += value;
            }
            remove
            {
                using (LockObject.EnterWriteLock())
                    _lstData.BeforeClearCollectionChanged -= value;
            }
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.BeforeClearCollectionChangedAsync" />
        public virtual event AsyncNotifyCollectionChangedEventHandler BeforeClearCollectionChangedAsync
        {
            add => _lstData.BeforeClearCollectionChangedAsync += value;
            remove => _lstData.BeforeClearCollectionChangedAsync -= value;
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.CollectionChangedAsync" />
        public virtual event AsyncNotifyCollectionChangedEventHandler CollectionChangedAsync
        {
            add => _lstData.CollectionChangedAsync += value;
            remove => _lstData.CollectionChangedAsync -= value;
        }

        private int _intIsDisposed;

        public bool IsDisposed => _intIsDisposed > 0;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Interlocked.CompareExchange(ref _intIsDisposed, 1, 0) > 0)
                    return;
                LockObject.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (disposing)
            {
                if (Interlocked.CompareExchange(ref _intIsDisposed, 1, 0) > 0)
                    return;
                await LockObject.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true).ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
