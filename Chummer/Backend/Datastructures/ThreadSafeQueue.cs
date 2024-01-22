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
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    public class ThreadSafeQueue<T> : IHasLockObject, IAsyncProducerConsumerCollection<T>, IAsyncCollection<T>, IAsyncReadOnlyCollection<T>
    {
        private readonly Queue<T> _queData;

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; }

        public ThreadSafeQueue(AsyncFriendlyReaderWriterLock objParentLock = null, bool blnLockReadOnlyForParent = false)
        {
            LockObject = new AsyncFriendlyReaderWriterLock(objParentLock, blnLockReadOnlyForParent);
            _queData = new Queue<T>();
        }

        public ThreadSafeQueue(IEnumerable<T> collection, AsyncFriendlyReaderWriterLock objParentLock = null, bool blnLockReadOnlyForParent = false)
        {
            LockObject = new AsyncFriendlyReaderWriterLock(objParentLock, blnLockReadOnlyForParent);
            _queData = new Queue<T>(collection);
        }

        public ThreadSafeQueue(int capacity, AsyncFriendlyReaderWriterLock objParentLock = null, bool blnLockReadOnlyForParent = false)
        {
            LockObject = new AsyncFriendlyReaderWriterLock(objParentLock, blnLockReadOnlyForParent);
            _queData = new Queue<T>(capacity);
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            LockingEnumerator<T> objReturn = LockingEnumerator<T>.Get(this);
            objReturn.SetEnumerator(_queData.GetEnumerator());
            return objReturn;
        }

        public async Task<IEnumerator<T>> GetEnumeratorAsync(CancellationToken token = default)
        {
            LockingEnumerator<T> objReturn = await LockingEnumerator<T>.GetAsync(this, token).ConfigureAwait(false);
            objReturn.SetEnumerator(_queData.GetEnumerator());
            return objReturn;
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(T item)
        {
            Enqueue(item);
        }

        /// <inheritdoc cref="Queue{T}.Clear" />
        public void Clear()
        {
            using (LockObject.EnterWriteLock())
                _queData.Clear();
        }

        /// <inheritdoc />
        public Task AddAsync(T item, CancellationToken token = default)
        {
            return EnqueueAsync(item, token);
        }

        /// <inheritdoc cref="Queue{T}.Clear" />
        public async Task ClearAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _queData.Clear();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="Queue{T}.Contains" />
        public bool Contains(T item)
        {
            using (LockObject.EnterReadLock())
                return _queData.Contains(item);
        }

        /// <inheritdoc cref="Queue{T}.Contains" />
        public async Task<bool> ContainsAsync(T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _queData.Contains(item);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public async Task<bool> RemoveAsync(T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (ReferenceEquals(await PeekAsync(token).ConfigureAwait(false), item))
                {
                    await DequeueAsync(token).ConfigureAwait(false);
                    return true;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            return false;
        }

        /// <inheritdoc cref="Queue{T}.TrimExcess" />
        public void TrimExcess()
        {
            using (LockObject.EnterWriteLock())
                _queData.TrimExcess();
        }

        /// <inheritdoc cref="Queue{T}.TrimExcess" />
        public async Task TrimExcessAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _queData.TrimExcess();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="Queue{T}.Peek" />
        public T Peek()
        {
            using (LockObject.EnterReadLock())
                return _queData.Peek();
        }

        /// <inheritdoc cref="Queue{T}.Peek" />
        public async Task<T> PeekAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _queData.Peek();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="Queue{T}.Dequeue" />
        public T Dequeue()
        {
            using (LockObject.EnterWriteLock())
                return _queData.Dequeue();
        }

        /// <inheritdoc cref="Queue{T}.Dequeue" />
        public async Task<T> DequeueAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _queData.Dequeue();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="Queue{T}.Enqueue" />
        public void Enqueue(T item)
        {
            using (LockObject.EnterWriteLock())
                _queData.Enqueue(item);
        }

        /// <inheritdoc cref="Queue{T}.Enqueue" />
        public async Task EnqueueAsync(T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _queData.Enqueue(item);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public bool TryTake(out T item)
        {
            using (LockObject.EnterReadLock())
            {
                if (_queData.Count == 0)
                {
                    item = default;
                    return false;
                }
            }
            using (LockObject.EnterUpgradeableReadLock())
            {
                if (_queData.Count > 0)
                {
                    // FIFO to be compliant with how the default for BlockingCollection<T> is ConcurrentQueue
                    using (LockObject.EnterWriteLock())
                        item = _queData.Dequeue();
                    return true;
                }
            }

            item = default;
            return false;
        }

        /// <inheritdoc cref="Queue{T}.ToArray" />
        public T[] ToArray()
        {
            using (LockObject.EnterReadLock())
                return _queData.ToArray();
        }

        /// <inheritdoc cref="Queue{T}.ToArray" />
        public async Task<T[]> ToArrayAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _queData.ToArray();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="Queue{T}.CopyTo" />
        public void CopyTo(T[] array, int index)
        {
            using (LockObject.EnterReadLock())
                _queData.CopyTo(array, index);
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            using (LockObject.EnterReadLock())
            {
                if (!ReferenceEquals(Peek(), item))
                    return false;
            }
            using (LockObject.EnterUpgradeableReadLock())
            {
                if (!ReferenceEquals(Peek(), item))
                    return false;
                Dequeue();
                return true;
            }
        }

        /// <inheritdoc cref="Queue{T}.CopyTo" />
        public async Task CopyToAsync(T[] array, int index, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _queData.CopyTo(array, index);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public bool TryAdd(T item)
        {
            Enqueue(item);
            return true;
        }

        public async Task<bool> TryAddAsync(T item, CancellationToken token = default)
        {
            await EnqueueAsync(item, token).ConfigureAwait(false);
            return true;
        }

        /// <inheritdoc />
        public async Task<Tuple<bool, T>> TryTakeAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_queData.Count == 0)
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
                if (_queData.Count > 0)
                {
                    T objReturn;
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        objReturn = _queData.Dequeue();
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

        /// <inheritdoc cref="Queue{T}.CopyTo" />
        public void CopyTo(Array array, int index)
        {
            using (LockObject.EnterReadLock())
            {
                foreach (T objItem in _queData)
                {
                    array.SetValue(objItem, index);
                    ++index;
                }
            }
        }

        /// <inheritdoc cref="Queue{T}.CopyTo" />
        public async Task CopyToAsync(Array array, int index, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                foreach (T objItem in _queData)
                {
                    array.SetValue(objItem, index);
                    ++index;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="Queue{T}.Count" />
        public int Count
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _queData.Count;
            }
        }

        /// <inheritdoc />
        public bool IsReadOnly => false;

        public async Task<int> GetCountAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _queData.Count;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="ICollection.SyncRoot" />
        public object SyncRoot => LockObject;

        /// <inheritdoc cref="ICollection.IsSynchronized" />
        public bool IsSynchronized => true;

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
    }
}
