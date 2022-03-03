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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Chummer
{
    public class LockingOrderedSet<T> : ISet<T>, IList<T>, IReadOnlyList<T>, IProducerConsumerCollection<T>, ISerializable, IDeserializationCallback, IHasLockObject
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
            LockingEnumerator<T> objReturn = new LockingEnumerator<T>(this);
            objReturn.SetEnumerator(_lstOrderedData.GetEnumerator());
            return objReturn;
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            LockingEnumerator<T> objReturn = new LockingEnumerator<T>(this);
            objReturn.SetEnumerator(_lstOrderedData.GetEnumerator());
            return objReturn;
        }

        /// <inheritdoc />
        public bool Add(T item)
        {
            using (EnterWriteLock.Enter(LockObject))
            {
                if (!_setData.Add(item))
                    return false;
                _lstOrderedData.Add(item);
            }
            return true;
        }

        public async ValueTask<bool> AddAsync(T item)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                if (!_setData.Add(item))
                    return false;
                _lstOrderedData.Add(item);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
            return true;
        }

        /// <inheritdoc />
        public void UnionWith(IEnumerable<T> other)
        {
            List<T> lstOther = other.ToList();
            using (EnterWriteLock.Enter(LockObject))
            {
                _lstOrderedData.AddRange(lstOther.Where(objItem => !_setData.Contains(objItem)));
                _setData.UnionWith(lstOther);
            }
        }

        /// <inheritdoc />
        public void IntersectWith(IEnumerable<T> other)
        {
            HashSet<T> setOther = other.ToHashSet();
            using (EnterWriteLock.Enter(LockObject))
            {
                _lstOrderedData.RemoveAll(objItem => !setOther.Contains(objItem));
                _setData.IntersectWith(setOther);
            }
        }

        /// <inheritdoc />
        public void ExceptWith(IEnumerable<T> other)
        {
            HashSet<T> setOther = other.ToHashSet();
            using (EnterWriteLock.Enter(LockObject))
            {
                _lstOrderedData.RemoveAll(objItem => setOther.Contains(objItem));
                _setData.ExceptWith(setOther);
            }
        }

        /// <inheritdoc />
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            HashSet<T> setOther = other.ToHashSet();
            using (EnterWriteLock.Enter(LockObject))
            {
                _lstOrderedData.RemoveAll(objItem => setOther.Contains(objItem));
                _lstOrderedData.AddRange(setOther.Where(objItem => !_setData.Contains(objItem)));
                _setData.SymmetricExceptWith(setOther);
            }
        }

        /// <inheritdoc />
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            using (EnterReadLock.Enter(LockObject))
                return _setData.IsSubsetOf(other);
        }

        /// <inheritdoc />
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            using (EnterReadLock.Enter(LockObject))
                return _setData.IsSupersetOf(other);
        }

        /// <inheritdoc />
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            using (EnterReadLock.Enter(LockObject))
                return _setData.IsProperSupersetOf(other);
        }

        /// <inheritdoc />
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            using (EnterReadLock.Enter(LockObject))
                return _setData.IsProperSubsetOf(other);
        }

        /// <inheritdoc />
        public bool Overlaps(IEnumerable<T> other)
        {
            using (EnterReadLock.Enter(LockObject))
                return _setData.Overlaps(other);
        }

        /// <inheritdoc />
        public bool SetEquals(IEnumerable<T> other)
        {
            using (EnterReadLock.Enter(LockObject))
                return _setData.SetEquals(other);
        }
        
        public async ValueTask UnionWithAsync(IEnumerable<T> other)
        {
            List<T> lstOther = other.ToList();
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                _lstOrderedData.AddRange(lstOther.Where(objItem => !_setData.Contains(objItem)));
                _setData.UnionWith(lstOther);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
        }
        
        public async ValueTask IntersectWithAsync(IEnumerable<T> other)
        {
            HashSet<T> setOther = other.ToHashSet();
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                _lstOrderedData.RemoveAll(objItem => !setOther.Contains(objItem));
                _setData.IntersectWith(setOther);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
        }
        
        public async ValueTask ExceptWithAsync(IEnumerable<T> other)
        {
            HashSet<T> setOther = other.ToHashSet();
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                _lstOrderedData.RemoveAll(objItem => setOther.Contains(objItem));
                _setData.ExceptWith(setOther);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
        }
        
        public async ValueTask SymmetricExceptWithAsync(IEnumerable<T> other)
        {
            HashSet<T> setOther = other.ToHashSet();
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                _lstOrderedData.RemoveAll(objItem => setOther.Contains(objItem));
                _lstOrderedData.AddRange(setOther.Where(objItem => !_setData.Contains(objItem)));
                _setData.SymmetricExceptWith(setOther);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
        }
        
        public async ValueTask<bool> IsSubsetOfAsync(IEnumerable<T> other)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _setData.IsSubsetOf(other);
        }
        
        public async ValueTask<bool> IsSupersetOfAsync(IEnumerable<T> other)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _setData.IsSupersetOf(other);
        }
        
        public async ValueTask<bool> IsProperSupersetOfAsync(IEnumerable<T> other)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _setData.IsProperSupersetOf(other);
        }
        
        public async ValueTask<bool> IsProperSubsetOfAsync(IEnumerable<T> other)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _setData.IsProperSubsetOf(other);
        }
        
        public async ValueTask<bool> OverlapsAsync(IEnumerable<T> other)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _setData.Overlaps(other);
        }
        
        public async ValueTask<bool> SetEqualsAsync(IEnumerable<T> other)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _setData.SetEquals(other);
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
            using (EnterWriteLock.Enter(LockObject))
            {
                _setData.Clear();
                _lstOrderedData.Clear();
            }
        }

        public async ValueTask ClearAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                _setData.Clear();
                _lstOrderedData.Clear();
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
        }

        /// <inheritdoc />
        public bool Contains(T item)
        {
            using (EnterReadLock.Enter(LockObject))
                return _setData.Contains(item);
        }

        public async ValueTask<bool> ContainsAsync(T item)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _setData.Contains(item);
        }

        /// <inheritdoc cref="ICollection.CopyTo" />
        public void CopyTo(T[] array, int arrayIndex)
        {
            using (EnterReadLock.Enter(LockObject))
                _lstOrderedData.CopyTo(array, arrayIndex);
        }

        public async ValueTask CopyToAsync(T[] array, int arrayIndex)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                _lstOrderedData.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool TryAdd(T item)
        {
            return Add(item);
        }

        public ValueTask<bool> TryAddAsync(T item)
        {
            return AddAsync(item);
        }

        /// <inheritdoc />
        public bool TryTake(out T item)
        {
            // Immediately enter a write lock to prevent attempted reads until we have either taken the item we want to take or failed to do so
            using (EnterWriteLock.Enter(LockObject))
            {
                if (_setData.Count > 0)
                {
                    // FIFO to be compliant with how the default for BlockingCollection<T> is ConcurrentQueue
                    item = _lstOrderedData[0];
                    if (_setData.Remove(item))
                    {
                        _lstOrderedData.RemoveAt(0);
                        return true;
                    }
                }
            }
            item = default;
            return false;
        }

        /// <inheritdoc />
        public T[] ToArray()
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstOrderedData.ToArray();
        }

        public async ValueTask<T[]> ToArrayAsync()
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstOrderedData.ToArray();
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            using (EnterWriteLock.Enter(LockObject))
            {
                if (!_setData.Remove(item))
                    return false;
                _lstOrderedData.Remove(item);
            }
            return true;
        }

        public async ValueTask<bool> RemoveAsync(T item)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                if (!_setData.Remove(item))
                    return false;
                _lstOrderedData.Remove(item);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
            return true;
        }

        /// <inheritdoc />
        public void CopyTo(Array array, int index)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                foreach (T objItem in _lstOrderedData)
                {
                    array.SetValue(objItem, index);
                    ++index;
                }
            }
        }

        public async ValueTask CopyToAsync(Array array, int index)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
            {
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
                using (EnterReadLock.Enter(LockObject))
                    return _lstOrderedData.Count;
            }
        }

        /// <inheritdoc />
        public object SyncRoot => LockObject;

        /// <inheritdoc />
        public bool IsSynchronized => true;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
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
                await LockObject.DisposeAsync();
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public int IndexOf(T item)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstOrderedData.IndexOf(item);
        }

        public async ValueTask<int> IndexOfAsync(T item)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstOrderedData.IndexOf(item);
        }

        /// <inheritdoc />
        public void Insert(int index, T item)
        {
            using (EnterWriteLock.Enter(LockObject))
            {
                if (!_setData.Add(item))
                    return;
                _lstOrderedData.Insert(index, item);
            }
        }

        public async ValueTask InsertAsync(int index, T item)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                if (!_setData.Add(item))
                    return;
                _lstOrderedData.Insert(index, item);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            using (EnterWriteLock.Enter(LockObject))
            {
                T objToRemove = _lstOrderedData[index];
                if (_setData.Remove(objToRemove))
                    _lstOrderedData.RemoveAt(index);
            }
        }

        public async ValueTask RemoveAtAsync(int index)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                T objToRemove = _lstOrderedData[index];
                if (_setData.Remove(objToRemove))
                    _lstOrderedData.RemoveAt(index);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
        }

        // ReSharper disable once InheritdocInvalidUsage
        /// <inheritdoc />
        public T this[int index]
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _lstOrderedData[index];
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    T objOldItem = _lstOrderedData[index];
                    if (objOldItem.Equals(value))
                        return;
                    using (EnterWriteLock.Enter(LockObject))
                    {
                        _setData.Remove(objOldItem);
                        _setData.Add(value);
                        _lstOrderedData[index] = value;
                    }
                }
            }
        }

        /// <inheritdoc />
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            using (EnterReadLock.Enter(LockObject))
                _setData.GetObjectData(info, context);
        }

        /// <inheritdoc />
        public void OnDeserialization(object sender)
        {
            using (EnterWriteLock.Enter(LockObject))
                _setData.OnDeserialization(sender);
        }

        /// <inheritdoc cref="List{T}.Sort()" />
        public void Sort()
        {
            using (EnterReadLock.Enter(LockObject))
            {
                if (_setData.Comparer is IComparer<T> comparer)
                {
                    using (EnterWriteLock.Enter(LockObject))
                        _lstOrderedData.Sort(comparer);
                }
                else
                {
                    using (EnterWriteLock.Enter(LockObject))
                        _lstOrderedData.Sort();
                }
            }
        }

        /// <inheritdoc cref="List{T}.Sort(Comparison{T})" />
        public void Sort(Comparison<T> comparison)
        {
            using (EnterWriteLock.Enter(LockObject))
                _lstOrderedData.Sort(comparison);
        }

        /// <inheritdoc cref="List{T}.Sort(IComparer{T})" />
        public void Sort(IComparer<T> comparer)
        {
            using (EnterWriteLock.Enter(LockObject))
                _lstOrderedData.Sort(comparer);
        }

        /// <inheritdoc cref="List{T}.Sort(int, int, IComparer{T})" />
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            using (EnterWriteLock.Enter(LockObject))
                _lstOrderedData.Sort(index, count, comparer);
        }

        /// <inheritdoc cref="List{T}.Sort()" />
        public async ValueTask SortAsync()
        {
            using (await EnterReadLock.EnterAsync(LockObject))
            {
                if (_setData.Comparer is IComparer<T> comparer)
                {
                    IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
                    try
                    {
                        _lstOrderedData.Sort(comparer);
                    }
                    finally
                    {
                        await objLocker.DisposeAsync();
                    }
                }
                else
                {
                    IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
                    try
                    {
                        _lstOrderedData.Sort();
                    }
                    finally
                    {
                        await objLocker.DisposeAsync();
                    }
                }
            }
        }

        /// <inheritdoc cref="List{T}.Sort(Comparison{T})" />
        public async ValueTask SortAsync(Comparison<T> comparison)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                _lstOrderedData.Sort(comparison);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
        }

        /// <inheritdoc cref="List{T}.Sort(IComparer{T})" />
        public async ValueTask SortAsync(IComparer<T> comparer)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                _lstOrderedData.Sort(comparer);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
        }

        /// <inheritdoc cref="List{T}.Sort(int, int, IComparer{T})" />
        public async ValueTask SortAsync(int index, int count, IComparer<T> comparer)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                _lstOrderedData.Sort(index, count, comparer);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
        }

        /// <inheritdoc cref="List{T}.Reverse(int, int)" />
        public void Reverse(int index, int count)
        {
            using (EnterWriteLock.Enter(LockObject))
                _lstOrderedData.Reverse(index, count);
        }

        /// <inheritdoc cref="List{T}.Reverse(int, int)" />
        public async ValueTask ReverseAsync(int index, int count)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                _lstOrderedData.Reverse(index, count);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
        }

        /// <inheritdoc cref="List{T}.FindAll" />
        public List<T> FindAll(Predicate<T> predicate)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstOrderedData.FindAll(predicate);
        }

        /// <inheritdoc cref="List{T}.FindAll" />
        public async ValueTask<List<T>> FindAllAsync(Predicate<T> predicate)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstOrderedData.FindAll(predicate);
        }
    }
}
