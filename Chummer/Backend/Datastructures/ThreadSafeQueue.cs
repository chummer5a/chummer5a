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
        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();

        public ThreadSafeQueue()
        {
            _queData = new Queue<T>();
        }

        public ThreadSafeQueue(IEnumerable<T> collection)
        {
            _queData = new Queue<T>(collection);
        }

        public ThreadSafeQueue(int capacity)
        {
            _queData = new Queue<T>(capacity);
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            LockingEnumerator<T> objReturn = LockingEnumerator<T>.Get(this);
            objReturn.SetEnumerator(_queData.GetEnumerator());
            return objReturn;
        }

        public async ValueTask<IEnumerator<T>> GetEnumeratorAsync(CancellationToken token = default)
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
        public ValueTask AddAsync(T item, CancellationToken token = default)
        {
            return EnqueueAsync(item, token);
        }

        /// <inheritdoc cref="Queue{T}.Clear" />
        public async ValueTask ClearAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
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
            using (EnterReadLock.Enter(LockObject))
                return _queData.Contains(item);
        }

        /// <inheritdoc cref="Queue{T}.Contains" />
        public async ValueTask<bool> ContainsAsync(T item, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return _queData.Contains(item);
        }

        /// <inheritdoc />
        public async ValueTask<bool> RemoveAsync(T item, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (ReferenceEquals(await PeekAsync(token).ConfigureAwait(false), item))
                {
                    await DequeueAsync(token).ConfigureAwait(false);
                    return true;
                }
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
        public async ValueTask TrimExcessAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
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
            using (EnterReadLock.Enter(LockObject))
                return _queData.Peek();
        }

        /// <inheritdoc cref="Queue{T}.Peek" />
        public async ValueTask<T> PeekAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return _queData.Peek();
        }

        /// <inheritdoc cref="Queue{T}.Dequeue" />
        public T Dequeue()
        {
            using (LockObject.EnterWriteLock())
                return _queData.Dequeue();
        }

        /// <inheritdoc cref="Queue{T}.Dequeue" />
        public async ValueTask<T> DequeueAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
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
        public async ValueTask EnqueueAsync(T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
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
            // Immediately enter a write lock to prevent attempted reads until we have either taken the item we want to take or failed to do so
            using (LockObject.EnterWriteLock())
            {
                if (_queData.Count > 0)
                {
                    // FIFO to be compliant with how the default for BlockingCollection<T> is ConcurrentQueue
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
            using (EnterReadLock.Enter(LockObject))
                return _queData.ToArray();
        }

        /// <inheritdoc cref="Queue{T}.ToArray" />
        public async ValueTask<T[]> ToArrayAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return _queData.ToArray();
        }

        /// <inheritdoc cref="Queue{T}.CopyTo" />
        public void CopyTo(T[] array, int index)
        {
            using (EnterReadLock.Enter(LockObject))
                _queData.CopyTo(array, index);
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                if (ReferenceEquals(Peek(), item))
                {
                    Dequeue();
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc cref="Queue{T}.CopyTo" />
        public async ValueTask CopyToAsync(T[] array, int index, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                _queData.CopyTo(array, index);
        }

        /// <inheritdoc />
        public bool TryAdd(T item)
        {
            Enqueue(item);
            return true;
        }

        public async ValueTask<bool> TryAddAsync(T item, CancellationToken token = default)
        {
            await EnqueueAsync(item, token).ConfigureAwait(false);
            return true;
        }

        /// <inheritdoc />
        public async ValueTask<Tuple<bool, T>> TryTakeAsync(CancellationToken token = default)
        {
            // Immediately enter a write lock to prevent attempted reads until we have either taken the item we want to take or failed to do so
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                if (_queData.Count > 0)
                {
                    return new Tuple<bool, T>(true, _queData.Dequeue());
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
            using (EnterReadLock.Enter(LockObject))
            {
                foreach (T objItem in _queData)
                {
                    array.SetValue(objItem, index);
                    ++index;
                }
            }
        }

        /// <inheritdoc cref="Queue{T}.CopyTo" />
        public async ValueTask CopyToAsync(Array array, int index, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                foreach (T objItem in _queData)
                {
                    array.SetValue(objItem, index);
                    ++index;
                }
            }
        }

        /// <inheritdoc cref="Queue{T}.Count" />
        public int Count
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _queData.Count;
            }
        }

        /// <inheritdoc />
        public bool IsReadOnly => false;

        public async ValueTask<int> GetCountAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return _queData.Count;
        }

        /// <inheritdoc cref="ICollection.SyncRoot" />
        public object SyncRoot => LockObject;

        /// <inheritdoc cref="ICollection.IsSynchronized" />
        public bool IsSynchronized => true;

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
