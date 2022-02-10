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
using System.Threading;

namespace Chummer
{
    public class LockingHashSet<T> : ISet<T>, IReadOnlyCollection<T>, IDisposable, IProducerConsumerCollection<T>, IHasLockingEnumerators<T>
    {
        private readonly HashSet<T> _setData;
        private readonly ReaderWriterLockSlim
            _rwlThis = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

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

        private readonly object _objActiveEnumeratorsLock = new object();
        private readonly List<LockingEnumerator<T>> _lstActiveEnumerators = new List<LockingEnumerator<T>>();

        public LockingEnumerator<T> CreateLockingEnumerator()
        {
            bool blnDoLock;
            LockingEnumerator<T> objReturn;
            lock (_objActiveEnumeratorsLock)
            {
                blnDoLock = _lstActiveEnumerators.Count == 0;
                objReturn = new LockingEnumerator<T>(_setData.GetEnumerator(), this);
                _lstActiveEnumerators.Add(objReturn);
            }
            if (blnDoLock)
                _rwlThis.EnterReadLock();
            return objReturn;
        }

        public void FreeLockingEnumerator(LockingEnumerator<T> objToFree)
        {
            bool blnFreeLock;
            lock (_objActiveEnumeratorsLock)
            {
                _lstActiveEnumerators.Remove(objToFree);
                blnFreeLock = _lstActiveEnumerators.Count == 0;
            }
            if (blnFreeLock)
                _rwlThis.ExitReadLock();
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return CreateLockingEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return CreateLockingEnumerator();
        }

        /// <inheritdoc />
        public bool Add(T item)
        {
            using (new EnterWriteLock(_rwlThis))
                return _setData.Add(item);
        }

        /// <inheritdoc />
        public void UnionWith(IEnumerable<T> other)
        {
            using (new EnterWriteLock(_rwlThis))
                _setData.UnionWith(other);
        }

        /// <inheritdoc />
        public void IntersectWith(IEnumerable<T> other)
        {
            using (new EnterWriteLock(_rwlThis))
                _setData.IntersectWith(other);
        }

        /// <inheritdoc />
        public void ExceptWith(IEnumerable<T> other)
        {
            using (new EnterWriteLock(_rwlThis))
                _setData.ExceptWith(other);
        }

        /// <inheritdoc />
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            using (new EnterWriteLock(_rwlThis))
                _setData.SymmetricExceptWith(other);
        }

        /// <inheritdoc />
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            using (new EnterReadLock(_rwlThis))
                return _setData.IsSubsetOf(other);
        }

        /// <inheritdoc />
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            using (new EnterReadLock(_rwlThis))
                return _setData.IsSupersetOf(other);
        }

        /// <inheritdoc />
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            using (new EnterReadLock(_rwlThis))
                return _setData.IsProperSupersetOf(other);
        }

        /// <inheritdoc />
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            using (new EnterReadLock(_rwlThis))
                return _setData.IsProperSubsetOf(other);
        }

        /// <inheritdoc />
        public bool Overlaps(IEnumerable<T> other)
        {
            using (new EnterReadLock(_rwlThis))
                return _setData.Overlaps(other);
        }

        /// <inheritdoc />
        public bool SetEquals(IEnumerable<T> other)
        {
            using (new EnterReadLock(_rwlThis))
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
            using (new EnterWriteLock(_rwlThis))
                _setData.Clear();
        }

        /// <inheritdoc />
        public bool Contains(T item)
        {
            using (new EnterReadLock(_rwlThis))
                return _setData.Contains(item);
        }

        /// <inheritdoc cref="ICollection.CopyTo" />
        public void CopyTo(T[] array, int arrayIndex)
        {
            using (new EnterReadLock(_rwlThis))
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
            using (new EnterWriteLock(_rwlThis))
                return _setData.Add(item);
        }

        /// <inheritdoc />
        public bool TryTake(out T item)
        {
            // Immediately enter a write lock to prevent attempted reads until we have either taken the item we want to take or failed to do so
            using (new EnterWriteLock(_rwlThis))
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
            using (new EnterReadLock(_rwlThis))
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
            using (new EnterWriteLock(_rwlThis))
                return _setData.Remove(item);
        }

        /// <inheritdoc />
        public void CopyTo(Array array, int index)
        {
            using (new EnterReadLock(_rwlThis))
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
                using (new EnterReadLock(_rwlThis))
                    return _setData.Count;
            }
        }

        /// <inheritdoc />
        public object SyncRoot => _rwlThis;

        /// <inheritdoc />
        public bool IsSynchronized => true;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                while (_rwlThis.IsReadLockHeld || _rwlThis.IsUpgradeableReadLockHeld || _rwlThis.IsUpgradeableReadLockHeld)
                    Utils.SafeSleep();
                _rwlThis.Dispose();
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
