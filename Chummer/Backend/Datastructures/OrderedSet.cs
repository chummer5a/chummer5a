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
using System.Linq;
using System.Runtime.Serialization;

namespace Chummer
{
    /// <summary>
    /// Like List that always only stores unique elements or a HashSet whose ordering is stored and tracked.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OrderedSet<T> : ISet<T>, IList<T>, IReadOnlyList<T>, ISerializable, IDeserializationCallback
    {
        private readonly HashSet<T> _setData;
        private readonly List<T> _lstOrderedData;

        public OrderedSet()
        {
            _setData = new HashSet<T>();
            _lstOrderedData = new List<T>();
        }

        public OrderedSet(IEnumerable<T> collection)
        {
            _setData = new HashSet<T>(collection);
            _lstOrderedData = new List<T>(_setData);
        }

        public OrderedSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            _setData = new HashSet<T>(collection, comparer);
            _lstOrderedData = new List<T>(_setData);
        }

        public OrderedSet(IEqualityComparer<T> comparer)
        {
            _setData = new HashSet<T>(comparer);
            _lstOrderedData = new List<T>();
        }

        public OrderedSet(int capacity)
        {
            _setData = new HashSet<T>(capacity);
            _lstOrderedData = new List<T>(capacity);
        }

        public OrderedSet(int capacity, IEqualityComparer<T> comparer)
        {
            _setData = new HashSet<T>(capacity, comparer);
            _lstOrderedData = new List<T>(capacity);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _lstOrderedData.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void UnionWith(IEnumerable<T> other)
        {
            List<T> lstOther = other.ToList();
            _lstOrderedData.AddRange(lstOther.Where(objItem => !_setData.Contains(objItem)));
            _setData.UnionWith(lstOther);
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            HashSet<T> setOther = other.ToHashSet();
            _lstOrderedData.RemoveAll(objItem => !setOther.Contains(objItem));
            _setData.IntersectWith(setOther);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            HashSet<T> setOther = other.ToHashSet();
            _lstOrderedData.RemoveAll(objItem => setOther.Contains(objItem));
            _setData.ExceptWith(setOther);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            HashSet<T> setOther = other.ToHashSet();
            _lstOrderedData.RemoveAll(objItem => setOther.Contains(objItem));
            _lstOrderedData.AddRange(setOther.Where(objItem => !_setData.Contains(objItem)));
            _setData.SymmetricExceptWith(setOther);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return _setData.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return _setData.IsSupersetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _setData.IsProperSupersetOf(other);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _setData.IsProperSubsetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return _setData.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return _setData.SetEquals(other);
        }

        void ICollection<T>.Add(T item)
        {
            if (!_setData.Add(item))
                return;
            _lstOrderedData.Add(item);
        }

        public bool Add(T item)
        {
            if (!_setData.Add(item))
                return false;
            _lstOrderedData.Add(item);
            return true;
        }

        public void Clear()
        {
            _setData.Clear();
            _lstOrderedData.Clear();
        }

        public bool Contains(T item)
        {
            return _setData.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _lstOrderedData.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            if (!_setData.Remove(item))
                return false;
            _lstOrderedData.Remove(item);
            return true;
        }

        public bool IsReadOnly => false;

        int ICollection<T>.Count => _lstOrderedData.Count;

        int IReadOnlyCollection<T>.Count => _lstOrderedData.Count;

        public int IndexOf(T item)
        {
            return _lstOrderedData.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _setData.Add(item);
            _lstOrderedData.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _setData.Remove(_lstOrderedData[index]);
            _lstOrderedData.RemoveAt(index);
        }

        public T this[int index]
        {
            get => _lstOrderedData[index];
            set
            {
                T objOldItem = _lstOrderedData[index];
                if (objOldItem.Equals(value))
                    return;
                _setData.Remove(objOldItem);
                _setData.Add(value);
                _lstOrderedData[index] = value;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            _setData.GetObjectData(info, context);
        }

        public void OnDeserialization(object sender)
        {
            _setData.OnDeserialization(sender);
        }

        public void Sort()
        {
            if (_setData.Comparer is IComparer<T> comparer)
                _lstOrderedData.Sort(comparer);
            else
                _lstOrderedData.Sort();
        }

        public void Sort(Comparison<T> comparison)
        {
            _lstOrderedData.Sort(comparison);
        }

        public void Sort(IComparer<T> comparer)
        {
            _lstOrderedData.Sort(comparer);
        }

        public void Sort(int index, int count, IComparer<T> comparer)
        {
            _lstOrderedData.Sort(index, count, comparer);
        }

        public void Reverse(int index, int count)
        {
            _lstOrderedData.Reverse(index, count);
        }

        public List<T> FindAll(Predicate<T> predicate)
        {
            return _lstOrderedData.FindAll(predicate);
        }
    }
}
