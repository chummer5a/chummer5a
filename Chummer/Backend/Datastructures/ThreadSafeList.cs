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
using System.Threading.Tasks;

namespace Chummer
{
    public class ThreadSafeList<T> : List<T>, IList<T>, IList, IProducerConsumerCollection<T>, IHasLockObject
    {
        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();

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
                using (EnterReadLock.Enter(LockObject))
                    return base.Capacity;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (base.Capacity == value)
                        return;
                    using (EnterWriteLock.Enter(LockObject))
                        base.Capacity = value;
                }
            }
        }

        /// <inheritdoc cref="List{T}.Count" />
        public new int Count
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
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
        object ICollection.SyncRoot => LockObject;

        /// <inheritdoc />
        public new T this[int index]
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return base[index];
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (base[index].Equals(value))
                        return;
                    using (EnterWriteLock.Enter(LockObject))
                        base[index] = value;
                }
            }
        }

        /// <inheritdoc />
        public new void Add(T item)
        {
            using (EnterWriteLock.Enter(LockObject))
                base.Add(item);
        }

        public async ValueTask AddAsync(T item)
        {
            using (await EnterWriteLock.EnterAsync(LockObject))
                base.Add(item);
        }

        /// <inheritdoc cref="List{T}.AddRange" />
        public new void AddRange(IEnumerable<T> collection)
        {
            using (EnterWriteLock.Enter(LockObject))
                base.AddRange(collection);
        }

        public async ValueTask AddRangeAsync(IEnumerable<T> collection)
        {
            using (await EnterWriteLock.EnterAsync(LockObject))
                base.AddRange(collection);
        }

        /// <inheritdoc cref="List{T}.AsReadOnly" />
        public new ReadOnlyCollection<T> AsReadOnly()
        {
            using (EnterReadLock.Enter(LockObject))
                return base.AsReadOnly();
        }

        /// <inheritdoc cref="List{T}.AsReadOnly" />
        public async ValueTask<ReadOnlyCollection<T>> AsReadOnlyAsync()
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.AsReadOnly();
        }

        /// <inheritdoc cref="List{T}.BinarySearch(int, int, T, IComparer{T})" />
        public new int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            using (EnterReadLock.Enter(LockObject))
                return base.BinarySearch(index, count, item, comparer);
        }

        /// <inheritdoc cref="List{T}.BinarySearch(T)" />
        public new int BinarySearch(T item)
        {
            using (EnterReadLock.Enter(LockObject))
                return base.BinarySearch(item);
        }

        /// <inheritdoc cref="List{T}.BinarySearch(T, IComparer{T})" />
        public new int BinarySearch(T item, IComparer<T> comparer)
        {
            using (EnterReadLock.Enter(LockObject))
                return base.BinarySearch(item, comparer);
        }

        /// <inheritdoc cref="List{T}.BinarySearch(int, int, T, IComparer{T})" />
        public async ValueTask<int> BinarySearchAsync(int index, int count, T item, IComparer<T> comparer)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.BinarySearch(index, count, item, comparer);
        }

        /// <inheritdoc cref="List{T}.BinarySearch(T)" />
        public async ValueTask<int> BinarySearchAsync(T item)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.BinarySearch(item);
        }

        /// <inheritdoc cref="List{T}.BinarySearch(T, IComparer{T})" />
        public async ValueTask<int> BinarySearchAsync(T item, IComparer<T> comparer)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.BinarySearch(item, comparer);
        }

        /// <inheritdoc cref="List{T}.Clear" />
        public new void Clear()
        {
            using (EnterWriteLock.Enter(LockObject))
                base.Clear();
        }

        /// <inheritdoc cref="List{T}.Clear" />
        public async ValueTask ClearAsync()
        {
            using (await EnterWriteLock.EnterAsync(LockObject))
                base.Clear();
        }

        /// <inheritdoc cref="List{T}.Contains" />
        public new bool Contains(T item)
        {
            using (EnterReadLock.Enter(LockObject))
                return base.Contains(item);
        }

        /// <inheritdoc cref="List{T}.Contains" />
        public async ValueTask<bool> ContainsAsync(T item)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.Contains(item);
        }

        /// <inheritdoc cref="List{T}.ConvertAll{TOutput}" />
        public new List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            using (EnterReadLock.Enter(LockObject))
                return base.ConvertAll(converter);
        }

        /// <inheritdoc cref="List{T}.ConvertAll{TOutput}" />
        public async ValueTask<List<TOutput>> ConvertAllAsync<TOutput>(Converter<T, TOutput> converter)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.ConvertAll(converter);
        }

        /// <inheritdoc cref="List{T}.CopyTo(T[])" />
        public new void CopyTo(T[] array)
        {
            using (EnterReadLock.Enter(LockObject))
                base.CopyTo(array);
        }

        /// <inheritdoc cref="List{T}.CopyTo(int, T[], int, int)" />
        public new void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            using (EnterReadLock.Enter(LockObject))
                base.CopyTo(index, array, arrayIndex, count);
        }

        /// <inheritdoc cref="List{T}.CopyTo(T[], int)" />
        public new void CopyTo(T[] array, int arrayIndex)
        {
            using (EnterReadLock.Enter(LockObject))
                base.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc cref="List{T}.CopyTo(T[])" />
        public async ValueTask CopyToAsync(T[] array)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                base.CopyTo(array);
        }

        /// <inheritdoc cref="List{T}.CopyTo(int, T[], int, int)" />
        public async ValueTask CopyToAsync(int index, T[] array, int arrayIndex, int count)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                base.CopyTo(index, array, arrayIndex, count);
        }

        /// <inheritdoc cref="List{T}.CopyTo(T[], int)" />
        public async ValueTask CopyToAsync(T[] array, int arrayIndex)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                base.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool TryAdd(T item)
        {
            Add(item);
            return true;
        }

        public async ValueTask<bool> TryAddAsync(T item)
        {
            await AddAsync(item);
            return true;
        }

        /// <inheritdoc />
        public bool TryTake(out T item)
        {
            // Immediately enter a write lock to prevent attempted reads until we have either taken the item we want to take or failed to do so
            using (EnterWriteLock.Enter(LockObject))
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
            using (EnterReadLock.Enter(LockObject))
                return base.Exists(match);
        }

        /// <inheritdoc cref="List{T}.Exists" />
        public async ValueTask<bool> ExistsAsync(Predicate<T> match)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.Exists(match);
        }

        /// <inheritdoc cref="List{T}.Find" />
        public new T Find(Predicate<T> match)
        {
            using (EnterReadLock.Enter(LockObject))
                return base.Find(match);
        }

        /// <inheritdoc cref="List{T}.Find" />
        public async ValueTask<T> FindAsync(Predicate<T> match)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.Find(match);
        }

        /// <inheritdoc cref="List{T}.FindAll" />
        public new List<T> FindAll(Predicate<T> match)
        {
            using (EnterReadLock.Enter(LockObject))
                return base.FindAll(match);
        }

        /// <inheritdoc cref="List{T}.FindAll" />
        public async ValueTask<List<T>> FindAllAsync(Predicate<T> match)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.FindAll(match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(Predicate{T})" />
        public new int FindIndex(Predicate<T> match)
        {
            using (EnterReadLock.Enter(LockObject))
                return base.FindIndex(match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(int, Predicate{T})" />
        public new int FindIndex(int startIndex, Predicate<T> match)
        {
            using (EnterReadLock.Enter(LockObject))
                return base.FindIndex(startIndex, match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(int, int, Predicate{T})" />
        public new int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            using (EnterReadLock.Enter(LockObject))
                return base.FindIndex(startIndex, count, match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(Predicate{T})" />
        public async ValueTask<int> FindIndexAsync(Predicate<T> match)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.FindIndex(match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(int, Predicate{T})" />
        public async ValueTask<int> FindIndexAsync(int startIndex, Predicate<T> match)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.FindIndex(startIndex, match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(int, int, Predicate{T})" />
        public async ValueTask<int> FindIndexAsync(int startIndex, int count, Predicate<T> match)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.FindIndex(startIndex, count, match);
        }

        /// <inheritdoc cref="List{T}.FindLast" />
        public new T FindLast(Predicate<T> match)
        {
            using (EnterReadLock.Enter(LockObject))
                return base.FindLast(match);
        }

        /// <inheritdoc cref="List{T}.FindLast" />
        public async ValueTask<T> FindLastAsync(Predicate<T> match)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.FindLast(match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(Predicate{T})" />
        public new int FindLastIndex(Predicate<T> match)
        {
            using (EnterReadLock.Enter(LockObject))
                return base.FindIndex(match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(int, Predicate{T})" />
        public new int FindLastIndex(int startIndex, Predicate<T> match)
        {
            using (EnterReadLock.Enter(LockObject))
                return base.FindIndex(startIndex, match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(int, int, Predicate{T})" />
        public new int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            using (EnterReadLock.Enter(LockObject))
                return base.FindLastIndex(startIndex, count, match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(Predicate{T})" />
        public async ValueTask<int> FindLastIndexAsync(Predicate<T> match)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.FindIndex(match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(int, Predicate{T})" />
        public async ValueTask<int> FindLastIndexAsync(int startIndex, Predicate<T> match)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.FindIndex(startIndex, match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(int, int, Predicate{T})" />
        public async ValueTask<int> FindLastIndexAsync(int startIndex, int count, Predicate<T> match)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.FindLastIndex(startIndex, count, match);
        }

        /// <inheritdoc cref="List{T}.ForEach" />
        public new void ForEach(Action<T> action)
        {
            using (EnterWriteLock.Enter(LockObject))
                base.ForEach(action);
        }

        /// <inheritdoc cref="List{T}.ForEach" />
        public async ValueTask ForEachAsync(Action<T> action)
        {
            using (await EnterWriteLock.EnterAsync(LockObject))
                base.ForEach(action);
        }

        /// <inheritdoc cref="List{T}.ForEach" />
        public async ValueTask ForEachAsync(Task<Action<T>> action)
        {
            using (await EnterWriteLock.EnterAsync(LockObject))
                base.ForEach(await action);
        }

        /// <inheritdoc cref="List{T}.GetEnumerator" />
        public new IEnumerator<T> GetEnumerator()
        {
            LockingEnumerator<T> objReturn = new LockingEnumerator<T>(this);
            objReturn.SetEnumerator(base.GetEnumerator());
            return objReturn;
        }

        /// <inheritdoc cref="List{T}.GetRange" />
        public new List<T> GetRange(int index, int count)
        {
            using (EnterReadLock.Enter(LockObject))
                return base.GetRange(index, count);
        }

        /// <inheritdoc cref="List{T}.GetRange" />
        public async ValueTask<List<T>> GetRangeAsync(int index, int count)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.GetRange(index, count);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T)" />
        public new int IndexOf(T item)
        {
            using (EnterReadLock.Enter(LockObject))
                return base.IndexOf(item);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T, int)" />
        public new int IndexOf(T item, int index)
        {
            using (EnterReadLock.Enter(LockObject))
                return base.IndexOf(item, index);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T, int, int)" />
        public new int IndexOf(T item, int index, int count)
        {
            using (EnterReadLock.Enter(LockObject))
                return base.IndexOf(item, index, count);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T)" />
        public async ValueTask<int> IndexOfAsync(T item)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.IndexOf(item);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T, int)" />
        public async ValueTask<int> IndexOfAsync(T item, int index)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.IndexOf(item, index);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T, int, int)" />
        public async ValueTask<int> IndexOfAsync(T item, int index, int count)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.IndexOf(item, index, count);
        }

        /// <inheritdoc cref="List{T}.Insert" />
        public new void Insert(int index, T item)
        {
            using (EnterWriteLock.Enter(LockObject))
                base.Insert(index, item);
        }

        /// <inheritdoc cref="List{T}.Insert" />
        public async ValueTask InsertAsync(int index, T item)
        {
            using (await EnterWriteLock.EnterAsync(LockObject))
                base.Insert(index, item);
        }

        /// <inheritdoc cref="List{T}.InsertRange" />
        public new void InsertRange(int index, IEnumerable<T> collection)
        {
            using (EnterWriteLock.Enter(LockObject))
                base.InsertRange(index, collection);
        }

        /// <inheritdoc cref="List{T}.InsertRange" />
        public async ValueTask InsertRangeAsync(int index, IEnumerable<T> collection)
        {
            using (await EnterWriteLock.EnterAsync(LockObject))
                base.InsertRange(index, collection);
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T)" />
        public new int LastIndexOf(T item)
        {
            using (EnterReadLock.Enter(LockObject))
                return base.LastIndexOf(item);
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T, int)" />
        public new int LastIndexOf(T item, int index)
        {
            using (EnterReadLock.Enter(LockObject))
                return base.LastIndexOf(item, index);
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T, int, int)" />
        public new int LastIndexOf(T item, int index, int count)
        {
            using (EnterReadLock.Enter(LockObject))
                return base.LastIndexOf(item, index, count);
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T)" />
        public async ValueTask<int> LastIndexOfAsync(T item)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.LastIndexOf(item);
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T, int)" />
        public async ValueTask<int> LastIndexOfAsync(T item, int index)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.LastIndexOf(item, index);
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T, int, int)" />
        public async ValueTask<int> LastIndexOfAsync(T item, int index, int count)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.LastIndexOf(item, index, count);
        }

        /// <inheritdoc />
        public new bool Remove(T item)
        {
            using (EnterReadLock.Enter(LockObject))
                return base.Remove(item);
        }

        public async ValueTask<bool> RemoveAsync(T item)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.Remove(item);
        }

        /// <inheritdoc cref="List{T}.RemoveAll" />
        public new int RemoveAll(Predicate<T> match)
        {
            using (EnterWriteLock.Enter(LockObject))
                return base.RemoveAll(match);
        }

        /// <inheritdoc cref="List{T}.RemoveAll" />
        public async ValueTask<int> RemoveAllAsync(Predicate<T> match)
        {
            using (await EnterWriteLock.EnterAsync(LockObject))
                return base.RemoveAll(match);
        }

        /// <inheritdoc cref="List{T}.RemoveAt" />
        public new void RemoveAt(int index)
        {
            using (EnterWriteLock.Enter(LockObject))
                base.RemoveAt(index);
        }

        /// <inheritdoc cref="List{T}.RemoveAt" />
        public async ValueTask RemoveAtAsync(int index)
        {
            using (await EnterWriteLock.EnterAsync(LockObject))
                base.RemoveAt(index);
        }

        /// <inheritdoc cref="List{T}.RemoveRange" />
        public new void RemoveRange(int index, int count)
        {
            using (EnterWriteLock.Enter(LockObject))
                base.RemoveRange(index, count);
        }

        /// <inheritdoc cref="List{T}.RemoveRange" />
        public async ValueTask RemoveRangeAsync(int index, int count)
        {
            using (await EnterWriteLock.EnterAsync(LockObject))
                base.RemoveRange(index, count);
        }

        /// <inheritdoc cref="List{T}.Reverse()" />
        public new void Reverse()
        {
            using (EnterWriteLock.Enter(LockObject))
                base.Reverse();
        }

        /// <inheritdoc cref="List{T}.Reverse(int, int)" />
        public new void Reverse(int index, int count)
        {
            using (EnterWriteLock.Enter(LockObject))
                base.Reverse(index, count);
        }

        /// <inheritdoc cref="List{T}.Reverse()" />
        public async ValueTask ReverseAsync()
        {
            using (await EnterWriteLock.EnterAsync(LockObject))
                base.Reverse();
        }

        /// <inheritdoc cref="List{T}.Reverse(int, int)" />
        public async ValueTask ReverseAsync(int index, int count)
        {
            using (await EnterWriteLock.EnterAsync(LockObject))
                base.Reverse(index, count);
        }

        /// <inheritdoc cref="List{T}.Sort()" />
        public new void Sort()
        {
            using (EnterWriteLock.Enter(LockObject))
                base.Sort();
        }

        /// <inheritdoc cref="List{T}.Sort(IComparer{T})" />
        public new void Sort(IComparer<T> comparer)
        {
            using (EnterWriteLock.Enter(LockObject))
                base.Sort(comparer);
        }

        /// <inheritdoc cref="List{T}.Sort(int, int, IComparer{T})" />
        public new void Sort(int index, int count, IComparer<T> comparer)
        {
            using (EnterWriteLock.Enter(LockObject))
                base.Sort(index, count, comparer);
        }

        /// <inheritdoc cref="List{T}.Sort(Comparison{T})" />
        public new void Sort(Comparison<T> comparison)
        {
            using (EnterWriteLock.Enter(LockObject))
                base.Sort(comparison);
        }

        /// <inheritdoc cref="List{T}.Sort()" />
        public async ValueTask SortAsync()
        {
            using (await EnterWriteLock.EnterAsync(LockObject))
                base.Sort();
        }

        /// <inheritdoc cref="List{T}.Sort(IComparer{T})" />
        public async ValueTask SortAsync(IComparer<T> comparer)
        {
            using (await EnterWriteLock.EnterAsync(LockObject))
                base.Sort(comparer);
        }

        /// <inheritdoc cref="List{T}.Sort(int, int, IComparer{T})" />
        public async ValueTask SortAsync(int index, int count, IComparer<T> comparer)
        {
            using (await EnterWriteLock.EnterAsync(LockObject))
                base.Sort(index, count, comparer);
        }

        /// <inheritdoc cref="List{T}.Sort(Comparison{T})" />
        public async ValueTask SortAsync(Comparison<T> comparison)
        {
            using (await EnterWriteLock.EnterAsync(LockObject))
                base.Sort(comparison);
        }

        /// <inheritdoc />
        public new T[] ToArray()
        {
            using (EnterReadLock.Enter(LockObject))
                return base.ToArray();
        }

        public async ValueTask<T[]> ToArrayAsync()
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.ToArray();
        }

        /// <inheritdoc cref="List{T}.TrimExcess" />
        public new void TrimExcess()
        {
            using (EnterWriteLock.Enter(LockObject))
                base.TrimExcess();
        }

        /// <inheritdoc cref="List{T}.TrimExcess" />
        public async ValueTask TrimExcessAsync()
        {
            using (await EnterWriteLock.EnterAsync(LockObject))
                base.TrimExcess();
        }

        /// <inheritdoc cref="List{T}.TrueForAll" />
        public new bool TrueForAll(Predicate<T> match)
        {
            using (EnterReadLock.Enter(LockObject))
                return base.TrueForAll(match);
        }

        /// <inheritdoc cref="List{T}.TrueForAll" />
        public async ValueTask<bool> TrueForAllAsync(Predicate<T> match)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.TrueForAll(match);
        }

        /// <inheritdoc cref="List{T}.TrueForAll" />
        public async ValueTask<bool> TrueForAllAsync(Task<Predicate<T>> match)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.TrueForAll(await match);
        }

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
    }
}
