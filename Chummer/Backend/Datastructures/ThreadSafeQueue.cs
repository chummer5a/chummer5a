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

namespace Chummer
{
    public class ThreadSafeQueue<T> : ICollection, IReadOnlyCollection<T>, IHasLockObject
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

        public void Clear()
        {
            using (new EnterWriteLock(LockObject))
                _queData.Clear();
        }

        public bool Contains(T item)
        {
            using (new EnterReadLock(LockObject))
                return _queData.Contains(item);
        }

        public void TrimExcess()
        {
            using (new EnterWriteLock(LockObject))
                _queData.TrimExcess();
        }

        public T Peek()
        {
            using (new EnterReadLock(LockObject))
                return _queData.Peek();
        }

        public T Dequeue()
        {
            using (new EnterWriteLock(LockObject))
                return _queData.Dequeue();
        }

        public void Enqueue(T item)
        {
            using (new EnterWriteLock(LockObject))
                _queData.Enqueue(item);
        }

        public T[] ToArray()
        {
            using (new EnterReadLock(LockObject))
                return _queData.ToArray();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            using (new EnterReadLock(LockObject))
                _queData.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public object SyncRoot => LockObject;

        /// <inheritdoc />
        public bool IsSynchronized => true;

        /// <inheritdoc />
        public void Dispose()
        {
            LockObject.Dispose();
        }
    }
}
