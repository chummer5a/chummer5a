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
using System.Threading.Tasks;

namespace Chummer
{
    public class LockingHashSet<T> : ISet<T>, IReadOnlyCollection<T>, IProducerConsumerCollection<T>, IHasLockObject, IAsyncEnumerable<T>
    {
        private readonly HashSet<T> _setData;
        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();

        public LockingHashSet()
        {
            _setData = new HashSet<T>();
        }

        public LockingHashSet(int capacity)
        {
            _setData = new HashSet<T>(capacity);
        }

        public LockingHashSet(IEnumerable<T> collection)
        {
            _setData = new HashSet<T>(collection);
        }

        public LockingHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            _setData = new HashSet<T>(collection, comparer);
        }

        public LockingHashSet(IEqualityComparer<T> comparer)
        {
            _setData = new HashSet<T>(comparer);
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            LockingEnumerator<T> objReturn = LockingEnumerator<T>.Get(this);
            objReturn.SetEnumerator(_setData.GetEnumerator());
            return objReturn;
        }

        public async ValueTask<IEnumerator<T>> GetEnumeratorAsync()
        {
            LockingEnumerator<T> objReturn = await LockingEnumerator<T>.GetAsync(this);
            objReturn.SetEnumerator(_setData.GetEnumerator());
            return objReturn;
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            LockingEnumerator<T> objReturn = LockingEnumerator<T>.Get(this);
            objReturn.SetEnumerator(_setData.GetEnumerator());
            return objReturn;
        }

        /// <inheritdoc />
        public bool Add(T item)
        {
            using (LockObject.EnterWriteLock())
                return _setData.Add(item);
        }

        public async ValueTask<bool> AddAsync(T item)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                return _setData.Add(item);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public void UnionWith(IEnumerable<T> other)
        {
            using (LockObject.EnterWriteLock())
                _setData.UnionWith(other);
        }

        /// <inheritdoc />
        public void IntersectWith(IEnumerable<T> other)
        {
            using (LockObject.EnterWriteLock())
                _setData.IntersectWith(other);
        }

        /// <inheritdoc />
        public void ExceptWith(IEnumerable<T> other)
        {
            using (LockObject.EnterWriteLock())
                _setData.ExceptWith(other);
        }

        /// <inheritdoc />
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            using (LockObject.EnterWriteLock())
                _setData.SymmetricExceptWith(other);
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
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                _setData.UnionWith(other);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }
        
        public async ValueTask IntersectWithAsync(IEnumerable<T> other)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                _setData.IntersectWith(other);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }
        
        public async ValueTask ExceptWithAsync(IEnumerable<T> other)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                _setData.ExceptWith(other);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }
        
        public async ValueTask SymmetricExceptWithAsync(IEnumerable<T> other)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                _setData.SymmetricExceptWith(other);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
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
            using (LockObject.EnterWriteLock())
                _setData.Clear();
        }

        public async ValueTask ClearAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                _setData.Clear();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
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
            {
                foreach (T objItem in _setData)
                {
                    array[arrayIndex] = objItem;
                    ++arrayIndex;
                }
            }
        }

        /// <inheritdoc cref="ICollection.CopyTo" />
        public async ValueTask CopyToAsync(T[] array, int arrayIndex)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
            {
                foreach (T objItem in _setData)
                {
                    array[arrayIndex] = objItem;
                    ++arrayIndex;
                }
            }
        }

        /// <inheritdoc />
        public bool TryAdd(T item)
        {
            using (LockObject.EnterWriteLock())
                return _setData.Add(item);
        }

        public async ValueTask<bool> TryAddAsync(T item)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                return _setData.Add(item);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public bool TryTake(out T item)
        {
            // Immediately enter a write lock to prevent attempted reads until we have either taken the item we want to take or failed to do so
            using (LockObject.EnterWriteLock())
            {
                if (_setData.Count > 0)
                {
                    // FIFO to be compliant with how the default for BlockingCollection<T> is ConcurrentQueue
                    item = _setData.First();
                    if (_setData.Remove(item))
                        return true;
                }
            }
            item = default;
            return false;
        }

        /// <inheritdoc />
        public T[] ToArray()
        {
            using (EnterReadLock.Enter(LockObject))
            {
                T[] aobjReturn = new T[_setData.Count];
                int i = 0;
                foreach (T objLoop in _setData)
                {
                    aobjReturn[i] = objLoop;
                    ++i;
                }
                return aobjReturn;
            }
        }

        public async ValueTask<T[]> ToArrayAsync()
        {
            using (await EnterReadLock.EnterAsync(LockObject))
            {
                T[] aobjReturn = new T[_setData.Count];
                int i = 0;
                foreach (T objLoop in _setData)
                {
                    aobjReturn[i] = objLoop;
                    ++i;
                }
                return aobjReturn;
            }
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            using (LockObject.EnterWriteLock())
                return _setData.Remove(item);
        }

        public async ValueTask<bool> RemoveAsync(T item)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                return _setData.Remove(item);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public void CopyTo(Array array, int index)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                foreach (T objItem in _setData)
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
                foreach (T objItem in _setData)
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
                    return _setData.Count;
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
    }
}
