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
using System.Threading;

namespace Chummer
{
    public class ThreadSafeQueue<T> : IReadOnlyCollection<T>, IHasLockObject, IProducerConsumerCollection<T>
    {
        private readonly Queue<T> _queData;

        /// <inheritdoc />
        public ReaderWriterLockSlim LockObject { get; } = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

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
            LockingEnumerator<T> objReturn = new LockingEnumerator<T>(this);
            objReturn.SetEnumerator(_queData.GetEnumerator());
            return objReturn;
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc cref="Queue{T}.Clear" />
        public void Clear()
        {
            using (new EnterWriteLock(LockObject))
                _queData.Clear();
        }

        /// <inheritdoc cref="Queue{T}.Contains" />
        public bool Contains(T item)
        {
            using (new EnterReadLock(LockObject))
                return _queData.Contains(item);
        }

        /// <inheritdoc cref="Queue{T}.TrimExcess" />
        public void TrimExcess()
        {
            using (new EnterWriteLock(LockObject))
                _queData.TrimExcess();
        }

        /// <inheritdoc cref="Queue{T}.Peek" />
        public T Peek()
        {
            using (new EnterReadLock(LockObject))
                return _queData.Peek();
        }

        /// <inheritdoc cref="Queue{T}.Dequeue" />
        public T Dequeue()
        {
            using (new EnterWriteLock(LockObject))
                return _queData.Dequeue();
        }

        /// <inheritdoc cref="Queue{T}.Enqueue" />
        public void Enqueue(T item)
        {
            using (new EnterWriteLock(LockObject))
                _queData.Enqueue(item);
        }

        /// <inheritdoc />
        public bool TryTake(out T item)
        {
            // Immediately enter a write lock to prevent attempted reads until we have either taken the item we want to take or failed to do so
            using (new EnterWriteLock(LockObject))
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
            using (new EnterReadLock(LockObject))
                return _queData.ToArray();
        }

        /// <inheritdoc cref="Queue{T}.CopyTo" />
        public void CopyTo(T[] array, int index)
        {
            using (new EnterReadLock(LockObject))
                _queData.CopyTo(array, index);
        }

        /// <inheritdoc />
        public bool TryAdd(T item)
        {
            Enqueue(item);
            return true;
        }

        /// <inheritdoc cref="Queue{T}.CopyTo" />
        public void CopyTo(Array array, int index)
        {
            using (new EnterReadLock(LockObject))
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
                using (new EnterReadLock(LockObject))
                    return _queData.Count;
            }
        }

        /// <inheritdoc cref="ICollection.SyncRoot" />
        public object SyncRoot => LockObject;

        /// <inheritdoc cref="ICollection.IsSynchronized" />
        public bool IsSynchronized => true;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                while (LockObject.IsReadLockHeld || LockObject.IsUpgradeableReadLockHeld || LockObject.IsUpgradeableReadLockHeld)
                    Utils.SafeSleep();
                LockObject.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
