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
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    public class LockingOrderedSet<T> : IAsyncSet<T>, IAsyncList<T>, IAsyncReadOnlyList<T>, IAsyncProducerConsumerCollection<T>, ISerializable, IDeserializationCallback, IHasLockObject
    {
        private readonly HashSet<T> _setData;
        private readonly List<T> _lstOrderedData;
        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();

        public LockingOrderedSet()
        {
            _setData = new HashSet<T>();
            _lstOrderedData = new List<T>();
        }

        public LockingOrderedSet(int capacity)
        {
            _setData = new HashSet<T>(capacity);
            _lstOrderedData = new List<T>(capacity);
        }

        public LockingOrderedSet(IEnumerable<T> collection)
        {
            _setData = new HashSet<T>(collection);
            _lstOrderedData = new List<T>(_setData);
        }

        public LockingOrderedSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            _setData = new HashSet<T>(collection, comparer);
            _lstOrderedData = new List<T>(_setData);
        }

        public LockingOrderedSet(IEqualityComparer<T> comparer)
        {
            _setData = new HashSet<T>(comparer);
            _lstOrderedData = new List<T>();
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            LockingEnumerator<T> objReturn = LockingEnumerator<T>.Get(this);
            objReturn.SetEnumerator(_lstOrderedData.GetEnumerator());
            return objReturn;
        }

        public async Task<IEnumerator<T>> GetEnumeratorAsync(CancellationToken token = default)
        {
            LockingEnumerator<T> objReturn = await LockingEnumerator<T>.GetAsync(this, token).ConfigureAwait(false);
            objReturn.SetEnumerator(_lstOrderedData.GetEnumerator());
            return objReturn;
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            LockingEnumerator<T> objReturn = LockingEnumerator<T>.Get(this);
            objReturn.SetEnumerator(_lstOrderedData.GetEnumerator());
            return objReturn;
        }

        /// <inheritdoc />
        public bool Add(T item)
        {
            using (LockObject.EnterWriteLock())
            {
                if (!_setData.Add(item))
                    return false;
                _lstOrderedData.Add(item);
            }
            return true;
        }

        public async Task<bool> AddAsync(T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!_setData.Add(item))
                    return false;
                _lstOrderedData.Add(item);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            return true;
        }

        /// <inheritdoc />
        async Task IAsyncCollection<T>.AddAsync(T item, CancellationToken token)
        {
            await AddAsync(item, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void UnionWith(IEnumerable<T> other)
        {
            List<T> lstOther = other.ToList();
            using (LockObject.EnterWriteLock())
            {
                _lstOrderedData.AddRange(lstOther.Where(objItem => !_setData.Contains(objItem)));
                _setData.UnionWith(lstOther);
            }
        }

        /// <inheritdoc />
        public void IntersectWith(IEnumerable<T> other)
        {
            HashSet<T> setOther = other.ToHashSet();
            using (LockObject.EnterWriteLock())
            {
                _lstOrderedData.RemoveAll(objItem => !setOther.Contains(objItem));
                _setData.IntersectWith(setOther);
            }
        }

        /// <inheritdoc />
        public void ExceptWith(IEnumerable<T> other)
        {
            HashSet<T> setOther = other.ToHashSet();
            using (LockObject.EnterWriteLock())
            {
                _lstOrderedData.RemoveAll(objItem => setOther.Contains(objItem));
                _setData.ExceptWith(setOther);
            }
        }

        /// <inheritdoc />
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            HashSet<T> setOther = other.ToHashSet();
            using (LockObject.EnterWriteLock())
            {
                _lstOrderedData.RemoveAll(objItem => setOther.Contains(objItem));
                _lstOrderedData.AddRange(setOther.Where(objItem => !_setData.Contains(objItem)));
                _setData.SymmetricExceptWith(setOther);
            }
        }

        /// <inheritdoc />
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            using (LockObject.EnterReadLock())
                return _setData.IsSubsetOf(other);
        }

        /// <inheritdoc />
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            using (LockObject.EnterReadLock())
                return _setData.IsSupersetOf(other);
        }

        /// <inheritdoc />
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            using (LockObject.EnterReadLock())
                return _setData.IsProperSupersetOf(other);
        }

        /// <inheritdoc />
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            using (LockObject.EnterReadLock())
                return _setData.IsProperSubsetOf(other);
        }

        /// <inheritdoc />
        public bool Overlaps(IEnumerable<T> other)
        {
            using (LockObject.EnterReadLock())
                return _setData.Overlaps(other);
        }

        /// <inheritdoc />
        public bool SetEquals(IEnumerable<T> other)
        {
            using (LockObject.EnterReadLock())
                return _setData.SetEquals(other);
        }

        public async Task UnionWithAsync(IEnumerable<T> other, CancellationToken token = default)
        {
            List<T> lstOther = other.ToList();
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _lstOrderedData.AddRange(lstOther.Where(objItem => !_setData.Contains(objItem)));
                _setData.UnionWith(lstOther);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task IntersectWithAsync(IEnumerable<T> other, CancellationToken token = default)
        {
            HashSet<T> setOther = other.ToHashSet();
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _lstOrderedData.RemoveAll(objItem => !setOther.Contains(objItem));
                _setData.IntersectWith(setOther);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task ExceptWithAsync(IEnumerable<T> other, CancellationToken token = default)
        {
            HashSet<T> setOther = other.ToHashSet();
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _lstOrderedData.RemoveAll(objItem => setOther.Contains(objItem));
                _setData.ExceptWith(setOther);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SymmetricExceptWithAsync(IEnumerable<T> other, CancellationToken token = default)
        {
            HashSet<T> setOther = other.ToHashSet();
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _lstOrderedData.RemoveAll(objItem => setOther.Contains(objItem));
                _lstOrderedData.AddRange(setOther.Where(objItem => !_setData.Contains(objItem)));
                _setData.SymmetricExceptWith(setOther);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task<bool> IsSubsetOfAsync(IEnumerable<T> other, CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                return _setData.IsSubsetOf(other);
            }
        }

        public async Task<bool> IsSupersetOfAsync(IEnumerable<T> other, CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                return _setData.IsSupersetOf(other);
            }
        }

        public async Task<bool> IsProperSupersetOfAsync(IEnumerable<T> other, CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                return _setData.IsProperSupersetOf(other);
            }
        }

        public async Task<bool> IsProperSubsetOfAsync(IEnumerable<T> other, CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                return _setData.IsProperSubsetOf(other);
            }
        }

        public async Task<bool> OverlapsAsync(IEnumerable<T> other, CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                return _setData.Overlaps(other);
            }
        }

        public async Task<bool> SetEqualsAsync(IEnumerable<T> other, CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                return _setData.SetEquals(other);
            }
        }

        /// <inheritdoc />
        void ICollection<T>.Add(T item)
        {
            if (item != null)
                Add(item);
        }

        /// <inheritdoc />
        public void Clear()
        {
            using (LockObject.EnterWriteLock())
            {
                _setData.Clear();
                _lstOrderedData.Clear();
            }
        }

        public async Task ClearAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _setData.Clear();
                _lstOrderedData.Clear();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public bool Contains(T item)
        {
            using (LockObject.EnterReadLock())
                return _setData.Contains(item);
        }

        public async Task<bool> ContainsAsync(T item, CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                return _setData.Contains(item);
            }
        }

        /// <inheritdoc cref="ICollection.CopyTo" />
        public void CopyTo(T[] array, int arrayIndex)
        {
            using (LockObject.EnterReadLock())
                _lstOrderedData.CopyTo(array, arrayIndex);
        }

        public async Task CopyToAsync(T[] array, int index, CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                _lstOrderedData.CopyTo(array, index);
            }
        }

        /// <inheritdoc />
        public bool TryAdd(T item)
        {
            return Add(item);
        }

        public Task<bool> TryAddAsync(T item, CancellationToken token = default)
        {
            return AddAsync(item, token);
        }

        /// <inheritdoc />
        public async Task<Tuple<bool, T>> TryTakeAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (_setData.Count == 0)
                    return new Tuple<bool, T>(false, default);
            }
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_setData.Count > 0)
                {
                    // FIFO to be compliant with how the default for BlockingCollection<T> is ConcurrentQueue
                    T objReturn = _lstOrderedData[0];
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (_setData.Remove(objReturn))
                        {
                            _lstOrderedData.RemoveAt(0);
                            return new Tuple<bool, T>(true, objReturn);
                        }
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            return new Tuple<bool, T>(false, default);
        }

        /// <inheritdoc />
        public bool TryTake(out T item)
        {
            using (LockObject.EnterReadLock())
            {
                if (_setData.Count == 0)
                {
                    item = default;
                    return false;
                }
            }
            using (LockObject.EnterUpgradeableReadLock())
            {
                if (_setData.Count > 0)
                {
                    // FIFO to be compliant with how the default for BlockingCollection<T> is ConcurrentQueue
                    item = _lstOrderedData[0];
                    using (LockObject.EnterWriteLock())
                    {
                        if (_setData.Remove(item))
                        {
                            _lstOrderedData.RemoveAt(0);
                            return true;
                        }
                    }
                }
            }
            item = default;
            return false;
        }

        /// <inheritdoc />
        public T[] ToArray()
        {
            using (LockObject.EnterReadLock())
                return _lstOrderedData.ToArray();
        }

        public async Task<T[]> ToArrayAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                return _lstOrderedData.ToArray();
            }
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            using (LockObject.EnterWriteLock())
            {
                if (!_setData.Remove(item))
                    return false;
                _lstOrderedData.Remove(item);
            }
            return true;
        }

        public async Task<bool> RemoveAsync(T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!_setData.Remove(item))
                    return false;
                _lstOrderedData.Remove(item);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            return true;
        }

        /// <inheritdoc />
        public void CopyTo(Array array, int index)
        {
            using (LockObject.EnterReadLock())
            {
                foreach (T objItem in _lstOrderedData)
                {
                    array.SetValue(objItem, index);
                    ++index;
                }
            }
        }

        public async Task CopyToAsync(Array array, int index, CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                foreach (T objItem in _lstOrderedData)
                {
                    array.SetValue(objItem, index);
                    ++index;
                }
            }
        }

        /// <inheritdoc cref="ICollection{T}.Count" />
        public int Count
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstOrderedData.Count;
            }
        }

        public async Task<int> GetCountAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                return _lstOrderedData.Count;
            }
        }

        /// <inheritdoc />
        public object SyncRoot => LockObject;

        /// <inheritdoc />
        public bool IsSynchronized => true;

        /// <inheritdoc />
        public bool IsReadOnly => false;

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

        /// <inheritdoc />
        public int IndexOf(T item)
        {
            using (LockObject.EnterReadLock())
                return _lstOrderedData.IndexOf(item);
        }

        public async Task<int> IndexOfAsync(T item, CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                return _lstOrderedData.IndexOf(item);
            }
        }

        /// <inheritdoc />
        public void Insert(int index, T item)
        {
            using (LockObject.EnterWriteLock())
            {
                if (!_setData.Add(item))
                    return;
                _lstOrderedData.Insert(index, item);
            }
        }

        public async Task InsertAsync(int index, T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!_setData.Add(item))
                    return;
                _lstOrderedData.Insert(index, item);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            using (LockObject.EnterWriteLock())
            {
                T objToRemove = _lstOrderedData[index];
                if (_setData.Remove(objToRemove))
                    _lstOrderedData.RemoveAt(index);
            }
        }

        public async Task RemoveAtAsync(int index, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                T objToRemove = _lstOrderedData[index];
                if (_setData.Remove(objToRemove))
                    _lstOrderedData.RemoveAt(index);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        // ReSharper disable once InheritdocInvalidUsage
        /// <inheritdoc />
        public T this[int index]
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstOrderedData[index];
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    T objOldItem = _lstOrderedData[index];
                    if (objOldItem.Equals(value))
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    T objOldItem = _lstOrderedData[index];
                    if (objOldItem.Equals(value))
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _setData.Remove(objOldItem);
                        _setData.Add(value);
                        _lstOrderedData[index] = value;
                    }
                }
            }
        }

        public async Task<T> GetValueAtAsync(int index, CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                return _lstOrderedData[index];
            }
        }

        public async Task SetValueAtAsync(int index, T value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                T objOldItem = _lstOrderedData[index];
                if (objOldItem.Equals(value))
                    return;
            }

            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                T objOldItem = _lstOrderedData[index];
                if (objOldItem.Equals(value))
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _setData.Remove(objOldItem);
                    _setData.Add(value);
                    _lstOrderedData[index] = value;
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
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            using (LockObject.EnterReadLock())
                _setData.GetObjectData(info, context);
        }

        /// <inheritdoc />
        public void OnDeserialization(object sender)
        {
            using (LockObject.EnterWriteLock())
                _setData.OnDeserialization(sender);
        }

        /// <inheritdoc cref="List{T}.Sort()" />
        public void Sort(CancellationToken token = default)
        {
            using (LockObject.EnterUpgradeableReadLock(token))
            {
                if (_lstOrderedData.Count > 0 && _lstOrderedData[0] is IHasLockObject)
                {
                    Stack<IDisposable> stkLockers = new Stack<IDisposable>(_lstOrderedData.Count);
                    try
                    {
                        foreach (IHasLockObject objTemp in _lstOrderedData.Select(v => (IHasLockObject)v))
                            stkLockers.Push(objTemp.LockObject.EnterReadLock(token));
                        if (_setData.Comparer is IComparer<T> comparer)
                        {
                            using (LockObject.EnterWriteLock(token))
                                _lstOrderedData.Sort(comparer);
                        }
                        else
                        {
                            using (LockObject.EnterWriteLock(token))
                                _lstOrderedData.Sort();
                        }
                    }
                    finally
                    {
                        while (stkLockers.Count > 0)
                        {
                            stkLockers.Pop()?.Dispose();
                        }
                    }
                }
                else if (_setData.Comparer is IComparer<T> comparer)
                {
                    using (LockObject.EnterWriteLock(token))
                        _lstOrderedData.Sort(comparer);
                }
                else
                {
                    using (LockObject.EnterWriteLock(token))
                        _lstOrderedData.Sort();
                }
            }
        }

        /// <inheritdoc cref="List{T}.Sort(Comparison{T})" />
        public void Sort(Comparison<T> comparison, CancellationToken token = default)
        {
            if (_lstOrderedData.Count > 0 && _lstOrderedData[0] is IHasLockObject)
            {
                Stack<IDisposable> stkLockers = new Stack<IDisposable>(_lstOrderedData.Count);
                try
                {
                    foreach (IHasLockObject objTemp in _lstOrderedData.Select(v => (IHasLockObject)v))
                        stkLockers.Push(objTemp.LockObject.EnterReadLock(token));
                    using (LockObject.EnterWriteLock(token))
                        _lstOrderedData.Sort(comparison);
                }
                finally
                {
                    while (stkLockers.Count > 0)
                    {
                        stkLockers.Pop()?.Dispose();
                    }
                }
            }
            else
            {
                using (LockObject.EnterWriteLock(token))
                    _lstOrderedData.Sort(comparison);
            }
        }

        /// <inheritdoc cref="List{T}.Sort(IComparer{T})" />
        public void Sort(IComparer<T> comparer, CancellationToken token = default)
        {
            if (_lstOrderedData.Count > 0 && _lstOrderedData[0] is IHasLockObject)
            {
                Stack<IDisposable> stkLockers = new Stack<IDisposable>(_lstOrderedData.Count);
                try
                {
                    foreach (IHasLockObject objTemp in _lstOrderedData.Select(v => (IHasLockObject)v))
                        stkLockers.Push(objTemp.LockObject.EnterReadLock(token));
                    using (LockObject.EnterWriteLock(token))
                        _lstOrderedData.Sort(comparer);
                }
                finally
                {
                    while (stkLockers.Count > 0)
                    {
                        stkLockers.Pop()?.Dispose();
                    }
                }
            }
            else
            {
                using (LockObject.EnterWriteLock(token))
                    _lstOrderedData.Sort(comparer);
            }
        }

        /// <inheritdoc cref="List{T}.Sort(int, int, IComparer{T})" />
        public void Sort(int index, int count, IComparer<T> comparer, CancellationToken token = default)
        {
            if (_lstOrderedData.Count > index && _lstOrderedData[0] is IHasLockObject)
            {
                Stack<IDisposable> stkLockers = new Stack<IDisposable>(count);
                try
                {
                    for (int i = 0; i < count; ++i)
                    {
                        if (_lstOrderedData[index + i] is IHasLockObject objTemp)
                            stkLockers.Push(objTemp.LockObject.EnterReadLock(token));
                    }

                    using (LockObject.EnterWriteLock(token))
                        _lstOrderedData.Sort(index, count, comparer);
                }
                finally
                {
                    while (stkLockers.Count > 0)
                    {
                        stkLockers.Pop()?.Dispose();
                    }
                }
            }
            else
            {
                using (LockObject.EnterWriteLock(token))
                    _lstOrderedData.Sort(index, count, comparer);
            }
        }

        /// <inheritdoc cref="List{T}.Sort()" />
        public async Task SortAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_lstOrderedData.Count > 0 && _lstOrderedData[0] is IHasLockObject)
                {
                    Stack<IDisposable> stkLockers = new Stack<IDisposable>(_lstOrderedData.Count);
                    try
                    {
                        foreach (IHasLockObject objTemp in _lstOrderedData.Select(v => (IHasLockObject)v))
                        {
                            token.ThrowIfCancellationRequested();
                            stkLockers.Push(await objTemp.LockObject.EnterReadLockAsync(token).ConfigureAwait(false));
                        }

                        if (_setData.Comparer is IComparer<T> comparer)
                        {
                            IAsyncDisposable objLocker2 =
                                await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                            try
                            {
                                token.ThrowIfCancellationRequested();
                                _lstOrderedData.Sort(comparer);
                            }
                            finally
                            {
                                await objLocker2.DisposeAsync().ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            IAsyncDisposable objLocker2 =
                                await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                            try
                            {
                                token.ThrowIfCancellationRequested();
                                _lstOrderedData.Sort();
                            }
                            finally
                            {
                                await objLocker2.DisposeAsync().ConfigureAwait(false);
                            }
                        }
                    }
                    finally
                    {
                        while (stkLockers.Count > 0)
                        {
                            IDisposable objTemp = stkLockers.Pop();
                            objTemp?.Dispose();
                        }
                    }
                }
                else if (_setData.Comparer is IComparer<T> comparer)
                {
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _lstOrderedData.Sort(comparer);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
                else
                {
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _lstOrderedData.Sort();
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
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
            if (_lstOrderedData.Count > 0 && _lstOrderedData[0] is IHasLockObject)
            {
                Stack<IDisposable> stkLockers = new Stack<IDisposable>(_lstOrderedData.Count);
                try
                {
                    foreach (IHasLockObject objTemp in _lstOrderedData.Select(v => (IHasLockObject)v))
                    {
                        token.ThrowIfCancellationRequested();
                        stkLockers.Push(await objTemp.LockObject.EnterReadLockAsync(token).ConfigureAwait(false));
                    }

                    IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _lstOrderedData.Sort(comparison);
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
                }
                finally
                {
                    while (stkLockers.Count > 0)
                    {
                        IDisposable objTemp = stkLockers.Pop();
                        objTemp?.Dispose();
                    }
                }
            }
            else
            {
                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _lstOrderedData.Sort(comparison);
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        /// <inheritdoc cref="List{T}.Sort(IComparer{T})" />
        public async Task SortAsync(IComparer<T> comparer, CancellationToken token = default)
        {
            if (_lstOrderedData.Count > 0 && _lstOrderedData[0] is IHasLockObject)
            {
                Stack<IDisposable> stkLockers = new Stack<IDisposable>(_lstOrderedData.Count);
                try
                {
                    foreach (IHasLockObject objTemp in _lstOrderedData.Select(v => (IHasLockObject)v))
                    {
                        token.ThrowIfCancellationRequested();
                        stkLockers.Push(await objTemp.LockObject.EnterReadLockAsync(token).ConfigureAwait(false));
                    }

                    IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _lstOrderedData.Sort(comparer);
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
                }
                finally
                {
                    while (stkLockers.Count > 0)
                    {
                        IDisposable objTemp = stkLockers.Pop();
                        objTemp?.Dispose();
                    }
                }
            }
            else
            {
                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _lstOrderedData.Sort(comparer);
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        /// <inheritdoc cref="List{T}.Sort(int, int, IComparer{T})" />
        public async Task SortAsync(int index, int count, IComparer<T> comparer, CancellationToken token = default)
        {
            if (_lstOrderedData.Count > 0 && _lstOrderedData[0] is IHasLockObject)
            {
                Stack<IDisposable> stkLockers = new Stack<IDisposable>(_lstOrderedData.Count);
                try
                {
                    for (int i = 0; i < count; ++i)
                    {
                        if (_lstOrderedData[index + i] is IHasLockObject objTemp)
                            stkLockers.Push(await objTemp.LockObject.EnterReadLockAsync(token).ConfigureAwait(false));
                    }

                    IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _lstOrderedData.Sort(index, count, comparer);
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
                }
                finally
                {
                    while (stkLockers.Count > 0)
                    {
                        IDisposable objTemp = stkLockers.Pop();
                        objTemp?.Dispose();
                    }
                }
            }
            else
            {
                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _lstOrderedData.Sort(index, count, comparer);
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        /// <inheritdoc cref="List{T}.Reverse(int, int)" />
        public void Reverse(int index, int count, CancellationToken token = default)
        {
            using (LockObject.EnterWriteLock(token))
                _lstOrderedData.Reverse(index, count);
        }

        /// <inheritdoc cref="List{T}.Reverse(int, int)" />
        public async Task ReverseAsync(int index, int count, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _lstOrderedData.Reverse(index, count);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.Find" />
        public T Find(Predicate<T> predicate, CancellationToken token = default)
        {
            using (LockObject.EnterReadLock(token))
                return _lstOrderedData.Find(predicate);
        }

        /// <inheritdoc cref="List{T}.Find" />
        public async Task<T> FindAsync(Predicate<T> predicate, CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                return _lstOrderedData.Find(predicate);
            }
        }

        /// <inheritdoc cref="List{T}.FindAll" />
        public List<T> FindAll(Predicate<T> predicate, CancellationToken token = default)
        {
            using (LockObject.EnterReadLock(token))
                return _lstOrderedData.FindAll(predicate);
        }

        /// <inheritdoc cref="List{T}.FindAll" />
        public async Task<List<T>> FindAllAsync(Predicate<T> predicate, CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                return _lstOrderedData.FindAll(predicate);
            }
        }
    }
}
