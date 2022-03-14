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
using System.Threading.Tasks;

namespace Chummer
{
    public class ThreadSafeStack<T> : IHasLockObject, IProducerConsumerCollection<T>, IAsyncReadOnlyCollection<T>
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

        public async ValueTask<IEnumerator<T>> GetEnumeratorAsync()
        {
            LockingEnumerator<T> objReturn = await LockingEnumerator<T>.GetAsync(this);
            objReturn.SetEnumerator(_stkData.GetEnumerator());
            return objReturn;
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc cref="Stack{T}.Clear"/>
        public void Clear()
        {
            using (LockObject.EnterWriteLock())
                _stkData.Clear();
        }

        /// <inheritdoc cref="Stack{T}.Clear"/>
        public async ValueTask ClearAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
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
        public async ValueTask<bool> ContainsAsync(T item)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _stkData.Contains(item);
        }

        /// <inheritdoc cref="Stack{T}.TrimExcess"/>
        public void TrimExcess()
        {
            using (LockObject.EnterWriteLock())
                _stkData.TrimExcess();
        }

        /// <inheritdoc cref="Stack{T}.TrimExcess"/>
        public async ValueTask TrimExcessAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
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
        public async ValueTask<T> PeekAsync()
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _stkData.Peek();
        }

        /// <inheritdoc cref="Stack{T}.Pop"/>
        public T Pop()
        {
            using (LockObject.EnterWriteLock())
                return _stkData.Pop();
        }

        /// <inheritdoc cref="Stack{T}.Pop"/>
        public async ValueTask<T> PopAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
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
        public async ValueTask PushAsync(T item)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
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

        public async ValueTask<bool> TryAddAsync(T item)
        {
            await PushAsync(item);
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

        public async ValueTask<Tuple<bool, T>> TryTakeAsync()
        {
            // Immediately enter a write lock to prevent attempted reads until we have either taken the item we want to take or failed to do so
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                if (_stkData.Count > 0)
                {
                    return new Tuple<bool, T>(true, _stkData.Pop());
                }
            }
            finally
            {
                await objLocker.DisposeAsync();
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
        public async ValueTask<T[]> ToArrayAsync()
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _stkData.ToArray();
        }

        /// <inheritdoc cref="Stack{T}.CopyTo"/>
        public void CopyTo(T[] array, int index)
        {
            using (EnterReadLock.Enter(LockObject))
                _stkData.CopyTo(array, index);
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
        public async ValueTask CopyToAsync(T[] array, int index)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                _stkData.CopyTo(array, index);
        }

        /// <inheritdoc cref="Stack{T}.CopyTo"/>
        public async ValueTask CopyToAsync(Array array, int index)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
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

        public ValueTask<int> CountAsync => GetCountAsync();

        private async ValueTask<int> GetCountAsync()
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _stkData.Count;
        }

        /// <inheritdoc />
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
