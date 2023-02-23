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
    public class ThreadSafeStack<T> : IHasLockObject, IAsyncProducerConsumerCollection<T>, IAsyncCollection<T>, IAsyncReadOnlyCollection<T>
    {
        private readonly Stack<T> _stkData;

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();

        public ThreadSafeStack()
        {
            _stkData = new Stack<T>();
        }

        public ThreadSafeStack(IEnumerable<T> collection)
        {
            _stkData = new Stack<T>(collection);
        }

        public ThreadSafeStack(int capacity)
        {
            _stkData = new Stack<T>(capacity);
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            LockingEnumerator<T> objReturn = LockingEnumerator<T>.Get(this);
            objReturn.SetEnumerator(_stkData.GetEnumerator());
            return objReturn;
        }

        public async ValueTask<IEnumerator<T>> GetEnumeratorAsync(CancellationToken token = default)
        {
            LockingEnumerator<T> objReturn = await LockingEnumerator<T>.GetAsync(this, token).ConfigureAwait(false);
            objReturn.SetEnumerator(_stkData.GetEnumerator());
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
            Push(item);
        }

        /// <inheritdoc cref="Stack{T}.Clear"/>
        public void Clear()
        {
            using (LockObject.EnterWriteLock())
                _stkData.Clear();
        }

        /// <inheritdoc />
        public ValueTask AddAsync(T item, CancellationToken token = default)
        {
            return PushAsync(item, token);
        }

        /// <inheritdoc cref="Stack{T}.Clear"/>
        public async ValueTask ClearAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                _stkData.Clear();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="Stack{T}.Contains"/>
        public bool Contains(T item)
        {
            using (EnterReadLock.Enter(LockObject))
                return _stkData.Contains(item);
        }

        /// <inheritdoc cref="Stack{T}.Contains"/>
        public async ValueTask<bool> ContainsAsync(T item, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return _stkData.Contains(item);
        }

        /// <inheritdoc cref="Stack{T}.TrimExcess"/>
        public void TrimExcess()
        {
            using (LockObject.EnterWriteLock())
                _stkData.TrimExcess();
        }

        /// <inheritdoc cref="Stack{T}.TrimExcess"/>
        public async ValueTask TrimExcessAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                _stkData.TrimExcess();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="Stack{T}.Peek"/>
        public T Peek()
        {
            using (EnterReadLock.Enter(LockObject))
                return _stkData.Peek();
        }

        /// <inheritdoc cref="Stack{T}.Peek"/>
        public async ValueTask<T> PeekAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return _stkData.Peek();
        }

        /// <inheritdoc cref="Stack{T}.Pop"/>
        public T Pop()
        {
            using (LockObject.EnterWriteLock())
                return _stkData.Pop();
        }

        /// <inheritdoc cref="Stack{T}.Pop"/>
        public async ValueTask<T> PopAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                return _stkData.Pop();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="Stack{T}.Push"/>
        public void Push(T item)
        {
            using (LockObject.EnterWriteLock())
                _stkData.Push(item);
        }

        /// <inheritdoc cref="Stack{T}.Push"/>
        public async ValueTask PushAsync(T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                _stkData.Push(item);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public bool TryAdd(T item)
        {
            Push(item);
            return true;
        }

        public async ValueTask<bool> TryAddAsync(T item, CancellationToken token = default)
        {
            await PushAsync(item, token).ConfigureAwait(false);
            return true;
        }

        /// <inheritdoc />
        public bool TryTake(out T item)
        {
            // Immediately enter a write lock to prevent attempted reads until we have either taken the item we want to take or failed to do so
            using (LockObject.EnterWriteLock())
            {
                if (_stkData.Count > 0)
                {
                    item = _stkData.Pop();
                    return true;
                }
            }

            item = default;
            return false;
        }

        public Tuple<bool, T> TryTake()
        {
            // Immediately enter a write lock to prevent attempted reads until we have either taken the item we want to take or failed to do so
            using (LockObject.EnterWriteLock())
            {
                if (_stkData.Count > 0)
                {
                    return new Tuple<bool, T>(true, _stkData.Pop());
                }
            }

            return new Tuple<bool, T>(false, default);
        }

        public async ValueTask<Tuple<bool, T>> TryTakeAsync(CancellationToken token = default)
        {
            // Immediately enter a write lock to prevent attempted reads until we have either taken the item we want to take or failed to do so
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                if (_stkData.Count > 0)
                {
                    return new Tuple<bool, T>(true, _stkData.Pop());
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            return new Tuple<bool, T>(false, default);
        }

        /// <inheritdoc cref="Stack{T}.ToArray"/>
        public T[] ToArray()
        {
            using (EnterReadLock.Enter(LockObject))
                return _stkData.ToArray();
        }

        /// <inheritdoc cref="Stack{T}.ToArray"/>
        public async ValueTask<T[]> ToArrayAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return _stkData.ToArray();
        }

        /// <inheritdoc cref="Stack{T}.CopyTo"/>
        public void CopyTo(T[] array, int index)
        {
            using (EnterReadLock.Enter(LockObject))
                _stkData.CopyTo(array, index);
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                if (ReferenceEquals(Peek(), item))
                {
                    Pop();
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public void CopyTo(Array array, int index)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                foreach (T objItem in _stkData)
                {
                    array.SetValue(objItem, index);
                    ++index;
                }
            }
        }

        /// <inheritdoc cref="Stack{T}.CopyTo"/>
        public async ValueTask CopyToAsync(T[] array, int index, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                _stkData.CopyTo(array, index);
        }

        /// <inheritdoc />
        public async ValueTask<bool> RemoveAsync(T item, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (ReferenceEquals(await PeekAsync(token).ConfigureAwait(false), item))
                {
                    await PopAsync(token).ConfigureAwait(false);
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc cref="Stack{T}.CopyTo"/>
        public async ValueTask CopyToAsync(Array array, int index, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                foreach (T objItem in _stkData)
                {
                    array.SetValue(objItem, index);
                    ++index;
                }
            }
        }

        /// <inheritdoc cref="Stack{T}.Count" />
        public int Count
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _stkData.Count;
            }
        }

        /// <inheritdoc />
        public bool IsReadOnly => false;

        public async ValueTask<int> GetCountAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return _stkData.Count;
        }

        /// <inheritdoc />
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
