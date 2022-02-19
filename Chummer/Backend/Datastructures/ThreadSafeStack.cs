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
    public class ThreadSafeStack<T> : IReadOnlyCollection<T>, IHasLockObject, IProducerConsumerCollection<T>
    {
        private readonly Stack<T> _stkData;

        /// <inheritdoc />
        public ReaderWriterLockSlim LockObject { get; } = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

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
            LockingEnumerator<T> objReturn = new LockingEnumerator<T>(this);
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
            using (new EnterWriteLock(LockObject))
                _stkData.Clear();
        }

        /// <inheritdoc cref="Stack{T}.Contains"/>
        public bool Contains(T item)
        {
            using (new EnterReadLock(LockObject))
                return _stkData.Contains(item);
        }

        /// <inheritdoc cref="Stack{T}.TrimExcess"/>
        public void TrimExcess()
        {
            using (new EnterWriteLock(LockObject))
                _stkData.TrimExcess();
        }

        /// <inheritdoc cref="Stack{T}.Peek"/>
        public T Peek()
        {
            using (new EnterReadLock(LockObject))
                return _stkData.Peek();
        }

        /// <inheritdoc cref="Stack{T}.Pop"/>
        public T Pop()
        {
            using (new EnterWriteLock(LockObject))
                return _stkData.Pop();
        }

        /// <inheritdoc cref="Stack{T}.Push"/>
        public void Push(T item)
        {
            using (new EnterWriteLock(LockObject))
                _stkData.Push(item);
        }

        /// <inheritdoc />
        public bool TryAdd(T item)
        {
            Push(item);
            return true;
        }

        /// <inheritdoc />
        public bool TryTake(out T item)
        {
            // Immediately enter a write lock to prevent attempted reads until we have either taken the item we want to take or failed to do so
            using (new EnterWriteLock(LockObject))
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

        /// <inheritdoc cref="Stack{T}.ToArray"/>
        public T[] ToArray()
        {
            using (new EnterReadLock(LockObject))
                return _stkData.ToArray();
        }

        /// <inheritdoc cref="Stack{T}.CopyTo"/>
        public void CopyTo(T[] array, int index)
        {
            using (new EnterReadLock(LockObject))
                _stkData.CopyTo(array, index);
        }

        /// <inheritdoc />
        public void CopyTo(Array array, int index)
        {
            using (new EnterReadLock(LockObject))
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
                using (new EnterReadLock(LockObject))
                    return _stkData.Count;
            }
        }

        /// <inheritdoc />
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
