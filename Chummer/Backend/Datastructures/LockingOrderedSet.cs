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
            using (new EnterWriteLock(LockObject))
            {
                if (!_setData.Add(item))
                    return false;
                _lstOrderedData.Add(item);
                return true;
            }
        }

        /// <inheritdoc />
        public void UnionWith(IEnumerable<T> other)
        {
            using (new EnterWriteLock(LockObject))
            {
                List<T> lstOther = other.ToList();
                _lstOrderedData.AddRange(lstOther.Where(objItem => !_setData.Contains(objItem)));
                _setData.UnionWith(lstOther);
            }
        }

        /// <inheritdoc />
        public void IntersectWith(IEnumerable<T> other)
        {
            using (new EnterWriteLock(LockObject))
            {
                HashSet<T> setOther = other.ToHashSet();
                _lstOrderedData.RemoveAll(objItem => !setOther.Contains(objItem));
                _setData.IntersectWith(setOther);
            }
        }

        /// <inheritdoc />
        public void ExceptWith(IEnumerable<T> other)
        {
            using (new EnterWriteLock(LockObject))
            {
                HashSet<T> setOther = other.ToHashSet();
                _lstOrderedData.RemoveAll(objItem => setOther.Contains(objItem));
                _setData.ExceptWith(setOther);
            }
        }

        /// <inheritdoc />
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            using (new EnterWriteLock(LockObject))
            {
                HashSet<T> setOther = other.ToHashSet();
                _lstOrderedData.RemoveAll(objItem => setOther.Contains(objItem));
                _lstOrderedData.AddRange(setOther.Where(objItem => !_setData.Contains(objItem)));
                _setData.SymmetricExceptWith(setOther);
            }
        }

        /// <inheritdoc />
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            using (new EnterReadLock(LockObject))
                return _setData.IsSubsetOf(other);
        }

        /// <inheritdoc />
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            using (new EnterReadLock(LockObject))
                return _setData.IsSupersetOf(other);
        }

        /// <inheritdoc />
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            using (new EnterReadLock(LockObject))
                return _setData.IsProperSupersetOf(other);
        }

        /// <inheritdoc />
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            using (new EnterReadLock(LockObject))
                return _setData.IsProperSubsetOf(other);
        }

        /// <inheritdoc />
        public bool Overlaps(IEnumerable<T> other)
        {
            using (new EnterReadLock(LockObject))
                return _setData.Overlaps(other);
        }

        /// <inheritdoc />
        public bool SetEquals(IEnumerable<T> other)
        {
            using (new EnterReadLock(LockObject))
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
            using (new EnterWriteLock(LockObject))
            {
                _setData.Clear();
                _lstOrderedData.Clear();
            }
        }

        /// <inheritdoc />
        public bool Contains(T item)
        {
            using (new EnterReadLock(LockObject))
                return _setData.Contains(item);
        }

        /// <inheritdoc cref="ICollection.CopyTo" />
        public void CopyTo(T[] array, int arrayIndex)
        {
            using (new EnterReadLock(LockObject))
                _lstOrderedData.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool TryAdd(T item)
        {
            return Add(item);
        }

        /// <inheritdoc />
        public bool TryTake(out T item)
        {
            // Immediately enter a write lock to prevent attempted reads until we have either taken the item we want to take or failed to do so
            using (new EnterWriteLock(LockObject))
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
            using (new EnterReadLock(LockObject))
                return _lstOrderedData.ToArray();
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            using (new EnterWriteLock(LockObject))
            {
                if (!_setData.Remove(item))
                    return false;
                _lstOrderedData.Remove(item);
                return true;
            }
        }

        /// <inheritdoc />
        public void CopyTo(Array array, int index)
        {
            using (new EnterReadLock(LockObject))
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
                using (new EnterReadLock(LockObject))
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
                while (LockObject.IsReadLockHeld || LockObject.IsWriteLockHeld)
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

        /// <inheritdoc />
        public int IndexOf(T item)
        {
            using (new EnterReadLock(LockObject))
                return _lstOrderedData.IndexOf(item);
        }

        /// <inheritdoc />
        public void Insert(int index, T item)
        {
            using (new EnterWriteLock(LockObject))
            {
                if (!_setData.Add(item))
                    return;
                _lstOrderedData.Insert(index, item);
            }
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            using (new EnterWriteLock(LockObject))
            {
                T objToRemove = _lstOrderedData[index];
                if (_setData.Remove(objToRemove))
                    _lstOrderedData.RemoveAt(index);
            }
        }

        // ReSharper disable once InheritdocInvalidUsage
        /// <inheritdoc />
        public T this[int index]
        {
            get
            {
                using (new EnterReadLock(LockObject))
                    return _lstOrderedData[index];
            }
            set
            {
                using (new EnterReadLock(LockObject))
                {
                    T objOldItem = _lstOrderedData[index];
                    if (objOldItem.Equals(value))
                        return;
                    using (new EnterWriteLock(LockObject))
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
            using (new EnterReadLock(LockObject))
                _setData.GetObjectData(info, context);
        }

        /// <inheritdoc />
        public void OnDeserialization(object sender)
        {
            using (new EnterWriteLock(LockObject))
                _setData.OnDeserialization(sender);
        }

        /// <inheritdoc cref="List{T}.Sort()" />
        public void Sort()
        {
            using (new EnterWriteLock(LockObject))
            {
                if (_setData.Comparer is IComparer<T> comparer)
                    _lstOrderedData.Sort(comparer);
                else
                    _lstOrderedData.Sort();
            }
        }

        /// <inheritdoc cref="List{T}.Sort(Comparison{T})" />
        public void Sort(Comparison<T> comparison)
        {
            using (new EnterWriteLock(LockObject))
                _lstOrderedData.Sort(comparison);
        }

        /// <inheritdoc cref="List{T}.Sort(IComparer{T})" />
        public void Sort(IComparer<T> comparer)
        {
            using (new EnterWriteLock(LockObject))
                _lstOrderedData.Sort(comparer);
        }

        /// <inheritdoc cref="List{T}.Sort(int, int, IComparer{T})" />
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            using (new EnterWriteLock(LockObject))
                _lstOrderedData.Sort(index, count, comparer);
        }

        /// <inheritdoc cref="List{T}.Reverse(int, int)" />
        public void Reverse(int index, int count)
        {
            using (new EnterWriteLock(LockObject))
                _lstOrderedData.Reverse(index, count);
        }

        /// <inheritdoc cref="List{T}.FindAll" />
        public List<T> FindAll(Predicate<T> predicate)
        {
            using (new EnterReadLock(LockObject))
                return _lstOrderedData.FindAll(predicate);
        }
    }
}
