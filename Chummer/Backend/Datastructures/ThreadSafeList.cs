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
        private readonly List<T> _lstInternal;

        public ThreadSafeList()
        {
            _lstInternal = new List<T>();
        }

        public ThreadSafeList(int capacity)
        {
            _lstInternal = new List<T>(capacity);
        }

        public ThreadSafeList(IEnumerable<T> collection)
        {
            _lstInternal = new List<T>(collection);
        }

        public new int Capacity
        {
            get
            {
                lock (_objLock)
                    return _lstInternal.Capacity;
            }
            set
            {
                lock (_objLock)
                    _lstInternal.Capacity = value;
            }
        }

        public new int Count
        {
            get
            {
                lock (_objLock)
                    return _lstInternal.Count;
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
                    return _lstInternal[index];
            }
            set
            {
                lock (_objLock)
                    _lstInternal[index] = value;
            }
        }

        public new void Add(T item)
        {
            lock (_objLock)
                _lstInternal.Add(item);
        }

        public new void AddRange(IEnumerable<T> collection)
        {
            lock (_objLock)
                _lstInternal.AddRange(collection);
        }

        public new ReadOnlyCollection<T> AsReadOnly()
        {
            lock (_objLock)
                return _lstInternal.AsReadOnly();
        }

        public new int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            lock (_objLock)
                return _lstInternal.BinarySearch(index, count, item, comparer);
        }

        public new int BinarySearch(T item)
        {
            lock (_objLock)
                return _lstInternal.BinarySearch(item);
        }

        public new int BinarySearch(T item, IComparer<T> comparer)
        {
            lock (_objLock)
                return _lstInternal.BinarySearch(item, comparer);
        }

        public new void Clear()
        {
            lock (_objLock)
                _lstInternal.Clear();
        }

        public new bool Contains(T item)
        {
            lock (_objLock)
                return _lstInternal.Contains(item);
        }

        public new List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            lock (_objLock)
                return _lstInternal.ConvertAll(converter);
        }

        public new void CopyTo(T[] array)
        {
            lock (_objLock)
                _lstInternal.CopyTo(array);
        }

        public new void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            lock (_objLock)
                _lstInternal.CopyTo(index, array, arrayIndex, count);
        }

        public new void CopyTo(T[] array, int arrayIndex)
        {
            lock (_objLock)
                _lstInternal.CopyTo(array, arrayIndex);
        }

        public new bool Exists(Predicate<T> match)
        {
            lock (_objLock)
                return _lstInternal.Exists(match);
        }

        public new T Find(Predicate<T> match)
        {
            lock (_objLock)
                return _lstInternal.Find(match);
        }

        public new List<T> FindAll(Predicate<T> match)
        {
            lock (_objLock)
                return _lstInternal.FindAll(match);
        }

        public new int FindIndex(Predicate<T> match)
        {
            lock (_objLock)
                return _lstInternal.FindIndex(match);
        }

        public new int FindIndex(int startIndex, Predicate<T> match)
        {
            lock (_objLock)
                return _lstInternal.FindIndex(startIndex, match);
        }

        public new int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            lock (_objLock)
                return _lstInternal.FindIndex(startIndex, count, match);
        }

        public new T FindLast(Predicate<T> match)
        {
            lock (_objLock)
                return _lstInternal.FindLast(match);
        }

        public new int FindLastIndex(Predicate<T> match)
        {
            lock (_objLock)
                return _lstInternal.FindIndex(match);
        }

        public new int FindLastIndex(int startIndex, Predicate<T> match)
        {
            lock (_objLock)
                return _lstInternal.FindIndex(startIndex, match);
        }

        public new int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            lock (_objLock)
                return _lstInternal.FindLastIndex(startIndex, count, match);
        }

        public new void ForEach(Action<T> action)
        {
            lock (_objLock)
                _lstInternal.ForEach(action);
        }

        public new Enumerator GetEnumerator()
        {
            lock (_objLock)
                return _lstInternal.GetEnumerator();
        }

        public new List<T> GetRange(int index, int count)
        {
            lock (_objLock)
                return _lstInternal.GetRange(index, count);
        }

        public new int IndexOf(T item)
        {
            lock (_objLock)
                return _lstInternal.IndexOf(item);
        }

        public new int IndexOf(T item, int index)
        {
            lock (_objLock)
                return _lstInternal.IndexOf(item, index);
        }

        public new int IndexOf(T item, int index, int count)
        {
            lock (_objLock)
                return _lstInternal.IndexOf(item, index, count);
        }

        public new void Insert(int index, T item)
        {
            lock (_objLock)
                _lstInternal.Insert(index, item);
        }

        public new void InsertRange(int index, IEnumerable<T> collection)
        {
            lock (_objLock)
                _lstInternal.InsertRange(index, collection);
        }

        public new int LastIndexOf(T item)
        {
            lock (_objLock)
                return _lstInternal.LastIndexOf(item);
        }

        public new int LastIndexOf(T item, int index)
        {
            lock (_objLock)
                return _lstInternal.LastIndexOf(item, index);
        }

        public new int LastIndexOf(T item, int index, int count)
        {
            lock (_objLock)
                return _lstInternal.LastIndexOf(item, index, count);
        }

        public new bool Remove(T item)
        {
            lock (_objLock)
                return _lstInternal.Remove(item);
        }

        public new int RemoveAll(Predicate<T> match)
        {
            lock (_objLock)
                return _lstInternal.RemoveAll(match);
        }

        public new void RemoveAt(int index)
        {
            lock (_objLock)
                _lstInternal.RemoveAt(index);
        }

        public new void RemoveRange(int index, int count)
        {
            lock (_objLock)
                _lstInternal.RemoveRange(index, count);
        }

        public new void Reverse()
        {
            lock (_objLock)
                _lstInternal.Reverse();
        }

        public new void Reverse(int index, int count)
        {
            lock (_objLock)
                _lstInternal.Reverse(index, count);
        }

        public new void Sort()
        {
            lock (_objLock)
                _lstInternal.Sort();
        }

        public new void Sort(IComparer<T> comparer)
        {
            lock (_objLock)
                _lstInternal.Sort(comparer);
        }

        public new void Sort(int index, int count, IComparer<T> comparer)
        {
            lock (_objLock)
                _lstInternal.Sort(index, count, comparer);
        }

        public new void Sort(Comparison<T> comparison)
        {
            lock (_objLock)
                _lstInternal.Sort(comparison);
        }

        public new T[] ToArray()
        {
            lock (_objLock)
                return _lstInternal.ToArray();
        }

        public new void TrimExcess()
        {
            lock (_objLock)
                _lstInternal.TrimExcess();
        }

        public new bool TrueForAll(Predicate<T> match)
        {
            lock (_objLock)
                return _lstInternal.TrueForAll(match);
        }
    }
}
