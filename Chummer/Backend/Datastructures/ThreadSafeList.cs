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
using System.Collections.ObjectModel;
using System.Threading;

namespace Chummer
{
    public class ThreadSafeList<T> : List<T>, IList<T>, IList, IDisposable, IProducerConsumerCollection<T>, IHasLockingEnumerators<T>
    {
        private readonly ReaderWriterLockSlim
            _rwlThis = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public ThreadSafeList()
        {
        }

        public ThreadSafeList(int capacity) : base(capacity)
        {
        }

        public ThreadSafeList(IEnumerable<T> collection) : base(collection)
        {
        }

        /// <inheritdoc cref="List{T}.Capacity" />
        public new int Capacity
        {
            get
            {
                using (new EnterReadLock(_rwlThis))
                    return base.Capacity;
            }
            set
            {
                using (new EnterUpgradeableReadLock(_rwlThis))
                {
                    if (base.Capacity == value)
                        return;
                    using (new EnterWriteLock(_rwlThis))
                        base.Capacity = value;
                }
            }
        }

        /// <inheritdoc cref="List{T}.Count" />
        public new int Count
        {
            get
            {
                using (new EnterReadLock(_rwlThis))
                    return base.Count;
            }
        }

        /// <inheritdoc />
        bool IList.IsFixedSize => false;
        /// <inheritdoc />
        bool ICollection<T>.IsReadOnly => false;
        /// <inheritdoc />
        bool IList.IsReadOnly => false;
        /// <inheritdoc />
        bool ICollection.IsSynchronized => true;
        /// <inheritdoc />
        object ICollection.SyncRoot => _rwlThis;

        /// <inheritdoc />
        public new T this[int index]
        {
            get
            {
                using (new EnterReadLock(_rwlThis))
                    return base[index];
            }
            set
            {
                using (new EnterUpgradeableReadLock(_rwlThis))
                {
                    if (base[index].Equals(value))
                        return;
                    using (new EnterWriteLock(_rwlThis))
                        base[index] = value;
                }
            }
        }

        /// <inheritdoc />
        public new void Add(T item)
        {
            using (new EnterWriteLock(_rwlThis))
                base.Add(item);
        }

        /// <inheritdoc cref="List{T}.AddRange" />
        public new void AddRange(IEnumerable<T> collection)
        {
            using (new EnterWriteLock(_rwlThis))
                base.AddRange(collection);
        }

        /// <inheritdoc cref="List{T}.AsReadOnly" />
        public new ReadOnlyCollection<T> AsReadOnly()
        {
            using (new EnterReadLock(_rwlThis))
                return base.AsReadOnly();
        }

        /// <inheritdoc cref="List{T}.BinarySearch(int, int, T, IComparer{T})" />
        public new int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            using (new EnterReadLock(_rwlThis))
                return base.BinarySearch(index, count, item, comparer);
        }

        /// <inheritdoc cref="List{T}.BinarySearch(T)" />
        public new int BinarySearch(T item)
        {
            using (new EnterReadLock(_rwlThis))
                return base.BinarySearch(item);
        }

        /// <inheritdoc cref="List{T}.BinarySearch(T, IComparer{T})" />
        public new int BinarySearch(T item, IComparer<T> comparer)
        {
            using (new EnterReadLock(_rwlThis))
                return base.BinarySearch(item, comparer);
        }

        /// <inheritdoc cref="List{T}.Clear" />
        public new void Clear()
        {
            using (new EnterWriteLock(_rwlThis))
                base.Clear();
        }

        /// <inheritdoc cref="List{T}.Contains" />
        public new bool Contains(T item)
        {
            using (new EnterReadLock(_rwlThis))
                return base.Contains(item);
        }

        /// <inheritdoc cref="List{T}.ConvertAll{TOutput}" />
        public new List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            using (new EnterReadLock(_rwlThis))
                return base.ConvertAll(converter);
        }

        /// <inheritdoc cref="List{T}.CopyTo(T[])" />
        public new void CopyTo(T[] array)
        {
            using (new EnterReadLock(_rwlThis))
                base.CopyTo(array);
        }

        /// <inheritdoc cref="List{T}.CopyTo(int, T[], int, int)" />
        public new void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            using (new EnterReadLock(_rwlThis))
                base.CopyTo(index, array, arrayIndex, count);
        }

        /// <inheritdoc cref="List{T}.CopyTo(T[], int)" />
        public new void CopyTo(T[] array, int arrayIndex)
        {
            using (new EnterReadLock(_rwlThis))
                base.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool TryAdd(T item)
        {
            Add(item);
            return true;
        }

        /// <inheritdoc />
        public bool TryTake(out T item)
        {
            // Immediately enter a write lock to prevent attempted reads until we have either taken the item we want to take or failed to do so
            using (new EnterWriteLock(_rwlThis))
            {
                if (base.Count > 0)
                {
                    // FIFO to be compliant with how the default for BlockingCollection<T> is ConcurrentQueue
                    item = base[0];
                    base.RemoveAt(0);
                    return true;
                }
            }

            item = default;
            return false;
        }

        /// <inheritdoc cref="List{T}.Exists" />
        public new bool Exists(Predicate<T> match)
        {
            using (new EnterReadLock(_rwlThis))
                return base.Exists(match);
        }

        /// <inheritdoc cref="List{T}.Find" />
        public new T Find(Predicate<T> match)
        {
            using (new EnterReadLock(_rwlThis))
                return base.Find(match);
        }

        /// <inheritdoc cref="List{T}.FindAll" />
        public new List<T> FindAll(Predicate<T> match)
        {
            using (new EnterReadLock(_rwlThis))
                return base.FindAll(match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(Predicate{T})" />
        public new int FindIndex(Predicate<T> match)
        {
            using (new EnterReadLock(_rwlThis))
                return base.FindIndex(match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(int, Predicate{T})" />
        public new int FindIndex(int startIndex, Predicate<T> match)
        {
            using (new EnterReadLock(_rwlThis))
                return base.FindIndex(startIndex, match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(int, int, Predicate{T})" />
        public new int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            using (new EnterReadLock(_rwlThis))
                return base.FindIndex(startIndex, count, match);
        }

        /// <inheritdoc cref="List{T}.FindLast" />
        public new T FindLast(Predicate<T> match)
        {
            using (new EnterReadLock(_rwlThis))
                return base.FindLast(match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(Predicate{T})" />
        public new int FindLastIndex(Predicate<T> match)
        {
            using (new EnterReadLock(_rwlThis))
                return base.FindIndex(match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(int, Predicate{T})" />
        public new int FindLastIndex(int startIndex, Predicate<T> match)
        {
            using (new EnterReadLock(_rwlThis))
                return base.FindIndex(startIndex, match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(int, int, Predicate{T})" />
        public new int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            using (new EnterReadLock(_rwlThis))
                return base.FindLastIndex(startIndex, count, match);
        }

        /// <inheritdoc cref="List{T}.ForEach" />
        public new void ForEach(Action<T> action)
        {
            using (new EnterWriteLock(_rwlThis))
                base.ForEach(action);
        }

        private readonly object _objActiveEnumeratorsLock = new object();
        private readonly List<LockingEnumerator<T>> _lstActiveEnumerators = new List<LockingEnumerator<T>>();

        public LockingEnumerator<T> CreateLockingEnumerator()
        {
            lock (_objActiveEnumeratorsLock)
            {
                bool blnDoLock = _lstActiveEnumerators.Count == 0;
                LockingEnumerator<T> objReturn = new LockingEnumerator<T>(base.GetEnumerator(), this);
                _lstActiveEnumerators.Add(objReturn);
                if (blnDoLock)
                    _rwlThis.EnterReadLock();
                return objReturn;
            }
        }

        public void FreeLockingEnumerator(LockingEnumerator<T> objToFree)
        {
            lock (_objActiveEnumeratorsLock)
            {
                _lstActiveEnumerators.Remove(objToFree);
                if (_lstActiveEnumerators.Count == 0)
                    _rwlThis.ExitReadLock();
            }
        }

        /// <inheritdoc cref="List{T}.GetEnumerator" />
        public new IEnumerator<T> GetEnumerator()
        {
            return CreateLockingEnumerator();
        }

        /// <inheritdoc cref="List{T}.GetRange" />
        public new List<T> GetRange(int index, int count)
        {
            using (new EnterReadLock(_rwlThis))
                return base.GetRange(index, count);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T)" />
        public new int IndexOf(T item)
        {
            using (new EnterReadLock(_rwlThis))
                return base.IndexOf(item);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T, int)" />
        public new int IndexOf(T item, int index)
        {
            using (new EnterReadLock(_rwlThis))
                return base.IndexOf(item, index);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T, int, int)" />
        public new int IndexOf(T item, int index, int count)
        {
            using (new EnterReadLock(_rwlThis))
                return base.IndexOf(item, index, count);
        }

        /// <inheritdoc cref="List{T}.Insert" />
        public new void Insert(int index, T item)
        {
            using (new EnterWriteLock(_rwlThis))
                base.Insert(index, item);
        }

        /// <inheritdoc cref="List{T}.InsertRange" />
        public new void InsertRange(int index, IEnumerable<T> collection)
        {
            using (new EnterWriteLock(_rwlThis))
                base.InsertRange(index, collection);
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T)" />
        public new int LastIndexOf(T item)
        {
            using (new EnterReadLock(_rwlThis))
                return base.LastIndexOf(item);
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T, int)" />
        public new int LastIndexOf(T item, int index)
        {
            using (new EnterReadLock(_rwlThis))
                return base.LastIndexOf(item, index);
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T, int, int)" />
        public new int LastIndexOf(T item, int index, int count)
        {
            using (new EnterReadLock(_rwlThis))
                return base.LastIndexOf(item, index, count);
        }

        /// <inheritdoc />
        public new bool Remove(T item)
        {
            using (new EnterReadLock(_rwlThis))
                return base.Remove(item);
        }

        /// <inheritdoc cref="List{T}.RemoveAll" />
        public new int RemoveAll(Predicate<T> match)
        {
            using (new EnterWriteLock(_rwlThis))
                return base.RemoveAll(match);
        }

        /// <inheritdoc cref="List{T}.RemoveAt" />
        public new void RemoveAt(int index)
        {
            using (new EnterWriteLock(_rwlThis))
                base.RemoveAt(index);
        }

        /// <inheritdoc cref="List{T}.RemoveRange" />
        public new void RemoveRange(int index, int count)
        {
            using (new EnterWriteLock(_rwlThis))
                base.RemoveRange(index, count);
        }

        /// <inheritdoc cref="List{T}.Reverse()" />
        public new void Reverse()
        {
            using (new EnterWriteLock(_rwlThis))
                base.Reverse();
        }

        /// <inheritdoc cref="List{T}.Reverse(int, int)" />
        public new void Reverse(int index, int count)
        {
            using (new EnterWriteLock(_rwlThis))
                base.Reverse(index, count);
        }

        /// <inheritdoc cref="List{T}.Sort()" />
        public new void Sort()
        {
            using (new EnterWriteLock(_rwlThis))
                base.Sort();
        }

        /// <inheritdoc cref="List{T}.Sort(IComparer{T})" />
        public new void Sort(IComparer<T> comparer)
        {
            using (new EnterWriteLock(_rwlThis))
                base.Sort(comparer);
        }

        /// <inheritdoc cref="List{T}.Sort(int, int, IComparer{T})" />
        public new void Sort(int index, int count, IComparer<T> comparer)
        {
            using (new EnterWriteLock(_rwlThis))
                base.Sort(index, count, comparer);
        }

        /// <inheritdoc cref="List{T}.Sort(Comparison{T})" />
        public new void Sort(Comparison<T> comparison)
        {
            using (new EnterWriteLock(_rwlThis))
                base.Sort(comparison);
        }

        /// <inheritdoc />
        public new T[] ToArray()
        {
            using (new EnterReadLock(_rwlThis))
                return base.ToArray();
        }

        /// <inheritdoc cref="List{T}.TrimExcess" />
        public new void TrimExcess()
        {
            using (new EnterWriteLock(_rwlThis))
                base.TrimExcess();
        }

        /// <inheritdoc cref="List{T}.TrueForAll" />
        public new bool TrueForAll(Predicate<T> match)
        {
            using (new EnterReadLock(_rwlThis))
                return base.TrueForAll(match);
        }

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
