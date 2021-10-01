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
using System.Collections.ObjectModel;
using System.Threading;

namespace Chummer
{
    public class ThreadSafeList<T> : List<T>, IList<T>, IList, IDisposable
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

        public new int Count
        {
            get
            {
                using (new EnterReadLock(_rwlThis))
                    return base.Count;
            }
        }

        bool IList.IsFixedSize => false;
        bool ICollection<T>.IsReadOnly => false;
        bool IList.IsReadOnly => false;
        bool ICollection.IsSynchronized => true;
        object ICollection.SyncRoot => _rwlThis;

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

        public new void Add(T item)
        {
            using (new EnterWriteLock(_rwlThis))
                base.Add(item);
        }

        public new void AddRange(IEnumerable<T> collection)
        {
            using (new EnterWriteLock(_rwlThis))
                base.AddRange(collection);
        }

        public new ReadOnlyCollection<T> AsReadOnly()
        {
            using (new EnterReadLock(_rwlThis))
                return base.AsReadOnly();
        }

        public new int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            using (new EnterReadLock(_rwlThis))
                return base.BinarySearch(index, count, item, comparer);
        }

        public new int BinarySearch(T item)
        {
            using (new EnterReadLock(_rwlThis))
                return base.BinarySearch(item);
        }

        public new int BinarySearch(T item, IComparer<T> comparer)
        {
            using (new EnterReadLock(_rwlThis))
                return base.BinarySearch(item, comparer);
        }

        public new void Clear()
        {
            using (new EnterWriteLock(_rwlThis))
                base.Clear();
        }

        public new bool Contains(T item)
        {
            using (new EnterReadLock(_rwlThis))
                return base.Contains(item);
        }

        public new List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            using (new EnterReadLock(_rwlThis))
                return base.ConvertAll(converter);
        }

        public new void CopyTo(T[] array)
        {
            using (new EnterReadLock(_rwlThis))
                base.CopyTo(array);
        }

        public new void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            using (new EnterReadLock(_rwlThis))
                base.CopyTo(index, array, arrayIndex, count);
        }

        public new void CopyTo(T[] array, int arrayIndex)
        {
            using (new EnterReadLock(_rwlThis))
                base.CopyTo(array, arrayIndex);
        }

        public new bool Exists(Predicate<T> match)
        {
            using (new EnterReadLock(_rwlThis))
                return base.Exists(match);
        }

        public new T Find(Predicate<T> match)
        {
            using (new EnterReadLock(_rwlThis))
                return base.Find(match);
        }

        public new List<T> FindAll(Predicate<T> match)
        {
            using (new EnterReadLock(_rwlThis))
                return base.FindAll(match);
        }

        public new int FindIndex(Predicate<T> match)
        {
            using (new EnterReadLock(_rwlThis))
                return base.FindIndex(match);
        }

        public new int FindIndex(int startIndex, Predicate<T> match)
        {
            using (new EnterReadLock(_rwlThis))
                return base.FindIndex(startIndex, match);
        }

        public new int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            using (new EnterReadLock(_rwlThis))
                return base.FindIndex(startIndex, count, match);
        }

        public new T FindLast(Predicate<T> match)
        {
            using (new EnterReadLock(_rwlThis))
                return base.FindLast(match);
        }

        public new int FindLastIndex(Predicate<T> match)
        {
            using (new EnterReadLock(_rwlThis))
                return base.FindIndex(match);
        }

        public new int FindLastIndex(int startIndex, Predicate<T> match)
        {
            using (new EnterReadLock(_rwlThis))
                return base.FindIndex(startIndex, match);
        }

        public new int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            using (new EnterReadLock(_rwlThis))
                return base.FindLastIndex(startIndex, count, match);
        }

        public new void ForEach(Action<T> action)
        {
            using (new EnterWriteLock(_rwlThis))
                base.ForEach(action);
        }

        public new Enumerator GetEnumerator()
        {
            using (new EnterReadLock(_rwlThis))
                return base.GetEnumerator();
        }

        public new List<T> GetRange(int index, int count)
        {
            using (new EnterReadLock(_rwlThis))
                return base.GetRange(index, count);
        }

        public new int IndexOf(T item)
        {
            using (new EnterReadLock(_rwlThis))
                return base.IndexOf(item);
        }

        public new int IndexOf(T item, int index)
        {
            using (new EnterReadLock(_rwlThis))
                return base.IndexOf(item, index);
        }

        public new int IndexOf(T item, int index, int count)
        {
            using (new EnterReadLock(_rwlThis))
                return base.IndexOf(item, index, count);
        }

        public new void Insert(int index, T item)
        {
            using (new EnterWriteLock(_rwlThis))
                base.Insert(index, item);
        }

        public new void InsertRange(int index, IEnumerable<T> collection)
        {
            using (new EnterWriteLock(_rwlThis))
                base.InsertRange(index, collection);
        }

        public new int LastIndexOf(T item)
        {
            using (new EnterReadLock(_rwlThis))
                return base.LastIndexOf(item);
        }

        public new int LastIndexOf(T item, int index)
        {
            using (new EnterReadLock(_rwlThis))
                return base.LastIndexOf(item, index);
        }

        public new int LastIndexOf(T item, int index, int count)
        {
            using (new EnterReadLock(_rwlThis))
                return base.LastIndexOf(item, index, count);
        }

        public new bool Remove(T item)
        {
            using (new EnterReadLock(_rwlThis))
                return base.Remove(item);
        }

        public new int RemoveAll(Predicate<T> match)
        {
            using (new EnterWriteLock(_rwlThis))
                return base.RemoveAll(match);
        }

        public new void RemoveAt(int index)
        {
            using (new EnterWriteLock(_rwlThis))
                base.RemoveAt(index);
        }

        public new void RemoveRange(int index, int count)
        {
            using (new EnterWriteLock(_rwlThis))
                base.RemoveRange(index, count);
        }

        public new void Reverse()
        {
            using (new EnterWriteLock(_rwlThis))
                base.Reverse();
        }

        public new void Reverse(int index, int count)
        {
            using (new EnterWriteLock(_rwlThis))
                base.Reverse(index, count);
        }

        public new void Sort()
        {
            using (new EnterWriteLock(_rwlThis))
                base.Sort();
        }

        public new void Sort(IComparer<T> comparer)
        {
            using (new EnterWriteLock(_rwlThis))
                base.Sort(comparer);
        }

        public new void Sort(int index, int count, IComparer<T> comparer)
        {
            using (new EnterWriteLock(_rwlThis))
                base.Sort(index, count, comparer);
        }

        public new void Sort(Comparison<T> comparison)
        {
            using (new EnterWriteLock(_rwlThis))
                base.Sort(comparison);
        }

        public new T[] ToArray()
        {
            using (new EnterReadLock(_rwlThis))
                return base.ToArray();
        }

        public new void TrimExcess()
        {
            using (new EnterWriteLock(_rwlThis))
                base.TrimExcess();
        }

        public new bool TrueForAll(Predicate<T> match)
        {
            using (new EnterReadLock(_rwlThis))
                return base.TrueForAll(match);
        }

        public void Dispose()
        {
            _rwlThis.Dispose();
        }
    }
}
