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

namespace Chummer
{
    public class ConcurrentHashSet<T> : ISet<T>, IReadOnlyCollection<T>, IProducerConsumerCollection<T>
    {
        protected ConcurrentDictionary<T, bool> DicInternal { get; }

        public ConcurrentHashSet()
        {
            DicInternal = new ConcurrentDictionary<T, bool>();
        }

        public ConcurrentHashSet(IEnumerable<T> collection)
        {
            DicInternal = new ConcurrentDictionary<T, bool>(collection.Select(x => new KeyValuePair<T, bool>(x, false)));
        }

        public ConcurrentHashSet(IEqualityComparer<T> comparer)
        {
            DicInternal = new ConcurrentDictionary<T, bool>(comparer);
        }

        public ConcurrentHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            DicInternal = new ConcurrentDictionary<T, bool>(collection.Select(x => new KeyValuePair<T, bool>(x, false)), comparer);
        }

        public ConcurrentHashSet(int concurrencyLevel, IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            DicInternal = new ConcurrentDictionary<T, bool>(concurrencyLevel, collection.Select(x => new KeyValuePair<T, bool>(x, false)), comparer);
        }

        public ConcurrentHashSet(
            int concurrencyLevel,
            int capacity,
            IEqualityComparer<T> comparer)
        {
            DicInternal = new ConcurrentDictionary<T, bool>(concurrencyLevel, capacity, comparer);
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return DicInternal.Keys.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public bool TryAdd(T item)
        {
            return item != null && DicInternal.TryAdd(item, false);
        }

        /// <inheritdoc />
        public bool TryTake(out T item)
        {
            if (!DicInternal.IsEmpty)
            {
                // FIFO to be compliant with how the default for BlockingCollection<T> is ConcurrentQueue
                item = DicInternal.Keys.First();
                if (DicInternal.TryRemove(item, out bool _))
                    return true;
            }
            item = default;
            return false;
        }

        /// <inheritdoc />
        public T[] ToArray()
        {
            return DicInternal.Keys.ToArray();
        }

        /// <inheritdoc />
        void ICollection<T>.Add(T item)
        {
            if (item != null)
                DicInternal.TryAdd(item, false);
        }

        /// <inheritdoc />
        public void UnionWith(IEnumerable<T> other)
        {
            foreach (T item in other)
            {
                DicInternal.TryAdd(item, false);
            }
        }

        /// <inheritdoc />
        public virtual void IntersectWith(IEnumerable<T> other)
        {
            HashSet<T> setOther = new HashSet<T>(other);
            foreach (T item in DicInternal.Keys)
            {
                if (!setOther.Contains(item))
                    DicInternal.TryRemove(item, out bool _);
            }
        }

        /// <inheritdoc />
        public void ExceptWith(IEnumerable<T> other)
        {
            foreach (T item in other)
            {
                DicInternal.TryRemove(item, out bool _);
            }
        }

        /// <inheritdoc />
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            foreach (T item in other)
            {
                if (!DicInternal.TryAdd(item, false))
                    DicInternal.TryRemove(item, out bool _);
            }
        }

        /// <inheritdoc />
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return other.Count(item => DicInternal.ContainsKey(item)) == DicInternal.Count;
        }

        /// <inheritdoc />
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return other.All(item => DicInternal.ContainsKey(item));
        }

        /// <inheritdoc />
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            int count = 0;
            foreach (T item in other)
            {
                if (!DicInternal.ContainsKey(item))
                    return false;
                ++count;
            }
            return count < DicInternal.Count;
        }

        /// <inheritdoc />
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            int count = 0;
            bool equals = true;
            foreach (T item in other)
            {
                if (DicInternal.ContainsKey(item))
                    ++count;
                else
                    equals = false;
            }
            return count == DicInternal.Count && !equals;
        }

        /// <inheritdoc />
        public bool Overlaps(IEnumerable<T> other)
        {
            return other.Any(item => DicInternal.ContainsKey(item));
        }

        /// <inheritdoc />
        public bool SetEquals(IEnumerable<T> other)
        {
            int count = 0;
            foreach (T item in other)
            {
                if (!DicInternal.ContainsKey(item))
                    return false;
                ++count;
            }
            return count == DicInternal.Count;
        }

        /// <inheritdoc />
        bool ISet<T>.Add(T item)
        {
            return DicInternal.TryAdd(item, false);
        }

        /// <inheritdoc />
        public void Clear()
        {
            DicInternal.Clear();
        }

        /// <inheritdoc />
        public bool Contains(T item)
        {
            return item != null && DicInternal.ContainsKey(item);
        }

        /// <inheritdoc cref="ISet{T}.CopyTo" />
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (arrayIndex + DicInternal.Count > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            for (int i = 0; i < DicInternal.Count; ++i)
            {
                array[arrayIndex] = DicInternal.Keys.ElementAt(i);
                ++arrayIndex;
            }
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            return item != null && DicInternal.TryRemove(item, out bool _);
        }

        /// <inheritdoc />
        public void CopyTo(Array array, int index)
        {
            if (index + DicInternal.Count > array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            for (int i = 0; i < DicInternal.Count; ++i)
            {
                array.SetValue(DicInternal.Keys.ElementAt(i), index);
                ++index;
            }
        }

        /// <inheritdoc cref="ISet{T}.Count" />
        public int Count => DicInternal.Count;

        /// <inheritdoc />
        public object SyncRoot => throw new NotSupportedException();

        /// <inheritdoc />
        public bool IsSynchronized => false;

        /// <inheritdoc />
        public bool IsReadOnly => false;
    }
}
