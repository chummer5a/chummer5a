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

namespace Chummer
{
    public class ThreadSafeList<T> : List<T>, IList<T>, IList
    {
        private readonly object _objLock = new object();

        public override string ToString()
        {
            lock (_objLock)
                return base.ToString();
        }

        public override int GetHashCode()
        {
            lock (_objLock)
                // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
                return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            lock (_objLock)
                // ReSharper disable once BaseObjectEqualsIsObjectEquals
                return base.Equals(obj);
        }

        public new int Capacity
        {
            get
            {
                lock (_objLock)
                    return base.Capacity;
            }
            set
            {
                lock (_objLock)
                    base.Capacity = value;
            }
        }
        
        public new int Count
        {
            get
            {
                lock (_objLock)
                    return base.Count;
            }
        }

        bool IList.IsFixedSize => false;
        bool ICollection<T>.IsReadOnly => false;
        bool IList.IsReadOnly => false;
        bool ICollection.IsSynchronized => true;
        object ICollection.SyncRoot => _objLock;

        public new T this[int index]
        {
            get
            {
                lock (_objLock)
                    return base[index];
            }
            set
            {
                lock (_objLock)
                    base[index] = value;
            }
        }

        public new void Add(T item)
        {
            lock (_objLock)
                base.Add(item);
        }

        public new void AddRange(IEnumerable<T> collection)
        {
            lock (_objLock)
                base.AddRange(collection);
        }

        public new ReadOnlyCollection<T> AsReadOnly()
        {
            lock (_objLock)
                return base.AsReadOnly();
        }

        public new int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            lock (_objLock)
                return base.BinarySearch(index, count, item, comparer);
        }

        public new int BinarySearch(T item)
        {
            lock (_objLock)
                return base.BinarySearch(item);
        }

        public new int BinarySearch(T item, IComparer<T> comparer)
        {
            lock (_objLock)
                return base.BinarySearch(item, comparer);
        }

        public new void Clear()
        {
            lock (_objLock)
                base.Clear();
        }

        public new bool Contains(T item)
        {
            lock (_objLock)
                return base.Contains(item);
        }

        public new List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            lock (_objLock)
                return base.ConvertAll(converter);
        }

        public new void CopyTo(T[] array)
        {
            lock (_objLock)
                base.CopyTo(array);
        }

        public new void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            lock (_objLock)
                base.CopyTo(index, array, arrayIndex, count);
        }

        public new void CopyTo(T[] array, int arrayIndex)
        {
            lock (_objLock)
                base.CopyTo(array, arrayIndex);
        }

        public new bool Exists(Predicate<T> match)
        {
            lock (_objLock)
                return base.Exists(match);
        }

        public new T Find(Predicate<T> match)
        {
            lock (_objLock)
                return base.Find(match);
        }

        public new List<T> FindAll(Predicate<T> match)
        {
            lock (_objLock)
                return base.FindAll(match);
        }

        public new int FindIndex(Predicate<T> match)
        {
            lock (_objLock)
                return base.FindIndex(match);
        }

        public new int FindIndex(int startIndex, Predicate<T> match)
        {
            lock (_objLock)
                return base.FindIndex(startIndex, match);
        }

        public new int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            lock (_objLock)
                return base.FindIndex(startIndex, count, match);
        }

        public new T FindLast(Predicate<T> match)
        {
            lock (_objLock)
                return base.FindLast(match);
        }

        public new int FindLastIndex(Predicate<T> match)
        {
            lock (_objLock)
                return base.FindIndex(match);
        }

        public new int FindLastIndex(int startIndex, Predicate<T> match)
        {
            lock (_objLock)
                return base.FindIndex(startIndex, match);
        }

        public new int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            lock (_objLock)
                return base.FindLastIndex(startIndex, count, match);
        }

        public new void ForEach(Action<T> action)
        {
            lock (_objLock)
                base.ForEach(action);
        }

        public new Enumerator GetEnumerator()
        {
            lock (_objLock)
                return base.GetEnumerator();
        }

        public new List<T> GetRange(int index, int count)
        {
            lock (_objLock)
                return base.GetRange(index, count);
        }

        public new int IndexOf(T item)
        {
            lock (_objLock)
                return base.IndexOf(item);
        }

        public new int IndexOf(T item, int index)
        {
            lock (_objLock)
                return base.IndexOf(item, index);
        }

        public new int IndexOf(T item, int index, int count)
        {
            lock (_objLock)
                return base.IndexOf(item, index, count);
        }

        public new void Insert(int index, T item)
        {
            lock (_objLock)
                base.Insert(index, item);
        }

        public new void InsertRange(int index, IEnumerable<T> collection)
        {
            lock (_objLock)
                base.InsertRange(index, collection);
        }

        public new int LastIndexOf(T item)
        {
            lock (_objLock)
                return base.LastIndexOf(item);
        }

        public new int LastIndexOf(T item, int index)
        {
            lock (_objLock)
                return base.LastIndexOf(item, index);
        }

        public new int LastIndexOf(T item, int index, int count)
        {
            lock (_objLock)
                return base.LastIndexOf(item, index, count);
        }

        public new bool Remove(T item)
        {
            lock (_objLock)
                return base.Remove(item);
        }

        public new int RemoveAll(Predicate<T> match)
        {
            lock (_objLock)
                return base.RemoveAll(match);
        }

        public new void RemoveAt(int index)
        {
            lock (_objLock)
                base.RemoveAt(index);
        }

        public new void RemoveRange(int index, int count)
        {
            lock (_objLock)
                base.RemoveRange(index, count);
        }

        public new void Reverse()
        {
            lock (_objLock)
                base.Reverse();
        }

        public new void Reverse(int index, int count)
        {
            lock (_objLock)
                base.Reverse(index, count);
        }

        public new void Sort()
        {
            lock (_objLock)
                base.Sort();
        }

        public new void Sort(IComparer<T> comparer)
        {
            lock (_objLock)
                base.Sort(comparer);
        }

        public new void Sort(int index, int count, IComparer<T> comparer)
        {
            lock (_objLock)
                base.Sort(index, count, comparer);
        }

        public new void Sort(Comparison<T> comparison)
        {
            lock (_objLock)
                base.Sort(comparison);
        }

        public new T[] ToArray()
        {
            lock (_objLock)
                return base.ToArray();
        }

        public new void TrimExcess()
        {
            lock (_objLock)
                base.TrimExcess();
        }

        public new bool TrueForAll(Predicate<T> match)
        {
            lock (_objLock)
                return base.TrueForAll(match);
        }
    }
}
