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
    public class ThreadSafeList<T> : IList<T>, IReadOnlyList<T>, IList, IProducerConsumerCollection<T>, IHasLockObject
    {
        private readonly List<T> _lstData;
        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();

        public ThreadSafeList()
        {
            _lstData = new List<T>();
        }

        public ThreadSafeList(int capacity)
        {
            _lstData = new List<T>(capacity);
        }

        public ThreadSafeList(IEnumerable<T> collection)
        {
            _lstData = new List<T>(collection);
        }

        /// <inheritdoc cref="List{T}.Capacity" />
        public int Capacity
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _lstData.Capacity;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_lstData.Capacity == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _lstData.Capacity = value;
                }
            }
        }
        
        public void CopyTo(Array array, int index)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                foreach (T objItem in _lstData)
                {
                    array.SetValue(objItem, index);
                    ++index;
                }
            }
        }
        
        public async ValueTask CopyToAsync(Array array, int index)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
            {
                foreach (T objItem in _lstData)
                {
                    array.SetValue(objItem, index);
                    ++index;
                }
            }
        }

        /// <inheritdoc cref="List{T}.Count" />
        public int Count
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _lstData.Count;
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
        
        public T this[int index]
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _lstData[index];
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_lstData[index].Equals(value))
                        return;
                    using (LockObject.EnterWriteLock())
                        _lstData[index] = value;
                }
            }
        }

        object IList.this[int index]
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _lstData[index];
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_lstData[index].Equals(value))
                        return;
                    using (LockObject.EnterWriteLock())
                        _lstData[index] = (T)value;
                }
            }
        }

        /// <inheritdoc />
        public void Add(T item)
        {
            using (LockObject.EnterWriteLock())
                _lstData.Add(item);
        }

        public async ValueTask AddAsync(T item)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                _lstData.Add(item);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.AddRange" />
        public void AddRange(IEnumerable<T> collection)
        {
            using (LockObject.EnterWriteLock())
                _lstData.AddRange(collection);
        }

        public async ValueTask AddRangeAsync(IEnumerable<T> collection)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                _lstData.AddRange(collection);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.AsReadOnly" />
        public ReadOnlyCollection<T> AsReadOnly()
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.AsReadOnly();
        }

        /// <inheritdoc cref="List{T}.AsReadOnly" />
        public async ValueTask<ReadOnlyCollection<T>> AsReadOnlyAsync()
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.AsReadOnly();
        }

        /// <inheritdoc cref="List{T}.BinarySearch(int, int, T, IComparer{T})" />
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.BinarySearch(index, count, item, comparer);
        }

        /// <inheritdoc cref="List{T}.BinarySearch(T)" />
        public int BinarySearch(T item)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.BinarySearch(item);
        }

        /// <inheritdoc cref="List{T}.BinarySearch(T, IComparer{T})" />
        public int BinarySearch(T item, IComparer<T> comparer)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.BinarySearch(item, comparer);
        }

        /// <inheritdoc cref="List{T}.BinarySearch(int, int, T, IComparer{T})" />
        public async ValueTask<int> BinarySearchAsync(int index, int count, T item, IComparer<T> comparer)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.BinarySearch(index, count, item, comparer);
        }

        /// <inheritdoc cref="List{T}.BinarySearch(T)" />
        public async ValueTask<int> BinarySearchAsync(T item)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.BinarySearch(item);
        }

        /// <inheritdoc cref="List{T}.BinarySearch(T, IComparer{T})" />
        public async ValueTask<int> BinarySearchAsync(T item, IComparer<T> comparer)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.BinarySearch(item, comparer);
        }

        public int Add(object value)
        {
            if (!(value is T objValue))
                return -1;
            using (EnterReadLock.Enter(LockObject))
            {
                Add(objValue);
                return _lstData.Count - 1;
            }
        }

        public bool Contains(object value)
        {
            if (!(value is T objValue))
                return false;
            return Contains(objValue);
        }

        /// <inheritdoc cref="List{T}.Clear" />
        public void Clear()
        {
            using (LockObject.EnterWriteLock())
                _lstData.Clear();
        }

        public int IndexOf(object value)
        {
            if (!(value is T objValue))
                return -1;
            return IndexOf(objValue);
        }

        public void Insert(int index, object value)
        {
            if (!(value is T objValue))
                return;
            Insert(index, objValue);
        }

        public void Remove(object value)
        {
            if (!(value is T objValue))
                return;
            Remove(objValue);
        }

        /// <inheritdoc cref="List{T}.Clear" />
        public async ValueTask ClearAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                _lstData.Clear();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.Contains" />
        public bool Contains(T item)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.Contains(item);
        }

        /// <inheritdoc cref="List{T}.Contains" />
        public async ValueTask<bool> ContainsAsync(T item)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.Contains(item);
        }

        /// <inheritdoc cref="List{T}.ConvertAll{TOutput}" />
        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.ConvertAll(converter);
        }

        /// <inheritdoc cref="List{T}.ConvertAll{TOutput}" />
        public async ValueTask<List<TOutput>> ConvertAllAsync<TOutput>(Converter<T, TOutput> converter)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.ConvertAll(converter);
        }

        /// <inheritdoc cref="List{T}.CopyTo(T[])" />
        public void CopyTo(T[] array)
        {
            using (EnterReadLock.Enter(LockObject))
                _lstData.CopyTo(array);
        }

        /// <inheritdoc cref="List{T}.CopyTo(int, T[], int, int)" />
        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            using (EnterReadLock.Enter(LockObject))
                _lstData.CopyTo(index, array, arrayIndex, count);
        }

        /// <inheritdoc cref="List{T}.CopyTo(T[], int)" />
        public void CopyTo(T[] array, int arrayIndex)
        {
            using (EnterReadLock.Enter(LockObject))
                _lstData.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc cref="List{T}.CopyTo(T[])" />
        public async ValueTask CopyToAsync(T[] array)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                _lstData.CopyTo(array);
        }

        /// <inheritdoc cref="List{T}.CopyTo(int, T[], int, int)" />
        public async ValueTask CopyToAsync(int index, T[] array, int arrayIndex, int count)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                _lstData.CopyTo(index, array, arrayIndex, count);
        }

        /// <inheritdoc cref="List{T}.CopyTo(T[], int)" />
        public async ValueTask CopyToAsync(T[] array, int arrayIndex)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                _lstData.CopyTo(array, arrayIndex);
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
            using (LockObject.EnterWriteLock())
            {
                if (_lstData.Count > 0)
                {
                    // FIFO to be compliant with how the default for BlockingCollection<T> is ConcurrentQueue
                    item = _lstData[0];
                    _lstData.RemoveAt(0);
                    return true;
                }
            }

            item = default;
            return false;
        }

        /// <inheritdoc cref="List{T}.Exists" />
        public bool Exists(Predicate<T> match)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.Exists(match);
        }

        /// <inheritdoc cref="List{T}.Exists" />
        public async ValueTask<bool> ExistsAsync(Predicate<T> match)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.Exists(match);
        }

        /// <inheritdoc cref="List{T}.Find" />
        public T Find(Predicate<T> match)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.Find(match);
        }

        /// <inheritdoc cref="List{T}.Find" />
        public async ValueTask<T> FindAsync(Predicate<T> match)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.Find(match);
        }

        /// <inheritdoc cref="List{T}.FindAll" />
        public List<T> FindAll(Predicate<T> match)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.FindAll(match);
        }

        /// <inheritdoc cref="List{T}.FindAll" />
        public async ValueTask<List<T>> FindAllAsync(Predicate<T> match)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.FindAll(match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(Predicate{T})" />
        public int FindIndex(Predicate<T> match)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.FindIndex(match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(int, Predicate{T})" />
        public int FindIndex(int startIndex, Predicate<T> match)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.FindIndex(startIndex, match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(int, int, Predicate{T})" />
        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.FindIndex(startIndex, count, match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(Predicate{T})" />
        public async ValueTask<int> FindIndexAsync(Predicate<T> match)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.FindIndex(match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(int, Predicate{T})" />
        public async ValueTask<int> FindIndexAsync(int startIndex, Predicate<T> match)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.FindIndex(startIndex, match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(int, int, Predicate{T})" />
        public async ValueTask<int> FindIndexAsync(int startIndex, int count, Predicate<T> match)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.FindIndex(startIndex, count, match);
        }

        /// <inheritdoc cref="List{T}.FindLast" />
        public T FindLast(Predicate<T> match)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.FindLast(match);
        }

        /// <inheritdoc cref="List{T}.FindLast" />
        public async ValueTask<T> FindLastAsync(Predicate<T> match)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.FindLast(match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(Predicate{T})" />
        public int FindLastIndex(Predicate<T> match)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.FindIndex(match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(int, Predicate{T})" />
        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.FindIndex(startIndex, match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(int, int, Predicate{T})" />
        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.FindLastIndex(startIndex, count, match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(Predicate{T})" />
        public async ValueTask<int> FindLastIndexAsync(Predicate<T> match)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.FindIndex(match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(int, Predicate{T})" />
        public async ValueTask<int> FindLastIndexAsync(int startIndex, Predicate<T> match)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.FindIndex(startIndex, match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(int, int, Predicate{T})" />
        public async ValueTask<int> FindLastIndexAsync(int startIndex, int count, Predicate<T> match)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.FindLastIndex(startIndex, count, match);
        }

        /// <inheritdoc cref="List{T}.ForEach" />
        public void ForEach(Action<T> action)
        {
            using (EnterReadLock.Enter(LockObject))
                _lstData.ForEach(action);
        }

        /// <inheritdoc cref="List{T}.ForEach" />
        public async ValueTask ForEachAsync(Action<T> action)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                _lstData.ForEach(action);
        }

        /// <inheritdoc cref="List{T}.ForEach" />
        public async ValueTask ForEachAsync(Task<Action<T>> action)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                _lstData.ForEach(await action);
        }

        /// <inheritdoc cref="List{T}.GetEnumerator" />
        public IEnumerator<T> GetEnumerator()
        {
            LockingEnumerator<T> objReturn = new LockingEnumerator<T>(this);
            objReturn.SetEnumerator(_lstData.GetEnumerator());
            return objReturn;
        }

        /// <inheritdoc cref="List{T}.GetRange" />
        public List<T> GetRange(int index, int count)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.GetRange(index, count);
        }

        /// <inheritdoc cref="List{T}.GetRange" />
        public async ValueTask<List<T>> GetRangeAsync(int index, int count)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.GetRange(index, count);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T)" />
        public int IndexOf(T item)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.IndexOf(item);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T, int)" />
        public int IndexOf(T item, int index)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.IndexOf(item, index);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T, int, int)" />
        public int IndexOf(T item, int index, int count)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.IndexOf(item, index, count);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T)" />
        public async ValueTask<int> IndexOfAsync(T item)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.IndexOf(item);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T, int)" />
        public async ValueTask<int> IndexOfAsync(T item, int index)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.IndexOf(item, index);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T, int, int)" />
        public async ValueTask<int> IndexOfAsync(T item, int index, int count)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.IndexOf(item, index, count);
        }

        /// <inheritdoc cref="List{T}.Insert" />
        public void Insert(int index, T item)
        {
            using (LockObject.EnterWriteLock())
                _lstData.Insert(index, item);
        }

        /// <inheritdoc cref="List{T}.Insert" />
        public async ValueTask InsertAsync(int index, T item)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                _lstData.Insert(index, item);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.InsertRange" />
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            using (LockObject.EnterWriteLock())
                _lstData.InsertRange(index, collection);
        }

        /// <inheritdoc cref="List{T}.InsertRange" />
        public async ValueTask InsertRangeAsync(int index, IEnumerable<T> collection)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                _lstData.InsertRange(index, collection);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T)" />
        public int LastIndexOf(T item)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.LastIndexOf(item);
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T, int)" />
        public int LastIndexOf(T item, int index)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.LastIndexOf(item, index);
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T, int, int)" />
        public int LastIndexOf(T item, int index, int count)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.LastIndexOf(item, index, count);
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T)" />
        public async ValueTask<int> LastIndexOfAsync(T item)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.LastIndexOf(item);
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T, int)" />
        public async ValueTask<int> LastIndexOfAsync(T item, int index)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.LastIndexOf(item, index);
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T, int, int)" />
        public async ValueTask<int> LastIndexOfAsync(T item, int index, int count)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.LastIndexOf(item, index, count);
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.Remove(item);
        }

        public async ValueTask<bool> RemoveAsync(T item)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.Remove(item);
        }

        /// <inheritdoc cref="List{T}.RemoveAll" />
        public int RemoveAll(Predicate<T> match)
        {
            using (LockObject.EnterWriteLock())
                return _lstData.RemoveAll(match);
        }

        /// <inheritdoc cref="List{T}.RemoveAll" />
        public async ValueTask<int> RemoveAllAsync(Predicate<T> match)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                return _lstData.RemoveAll(match);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.RemoveAll" />
        public async ValueTask<int> RemoveAllAsync(Task<Predicate<T>> match)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                return _lstData.RemoveAll(await match);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.RemoveAt" />
        public void RemoveAt(int index)
        {
            using (LockObject.EnterWriteLock())
                _lstData.RemoveAt(index);
        }

        /// <inheritdoc cref="List{T}.RemoveAt" />
        public async ValueTask RemoveAtAsync(int index)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                _lstData.RemoveAt(index);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.RemoveRange" />
        public void RemoveRange(int index, int count)
        {
            using (LockObject.EnterWriteLock())
                _lstData.RemoveRange(index, count);
        }

        /// <inheritdoc cref="List{T}.RemoveRange" />
        public async ValueTask RemoveRangeAsync(int index, int count)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                _lstData.RemoveRange(index, count);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.Reverse()" />
        public void Reverse()
        {
            using (LockObject.EnterWriteLock())
                _lstData.Reverse();
        }

        /// <inheritdoc cref="List{T}.Reverse(int, int)" />
        public void Reverse(int index, int count)
        {
            using (LockObject.EnterWriteLock())
                _lstData.Reverse(index, count);
        }

        /// <inheritdoc cref="List{T}.Reverse()" />
        public async ValueTask ReverseAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                _lstData.Reverse();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.Reverse(int, int)" />
        public async ValueTask ReverseAsync(int index, int count)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                _lstData.Reverse(index, count);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.Sort()" />
        public void Sort()
        {
            using (LockObject.EnterWriteLock())
                _lstData.Sort();
        }

        /// <inheritdoc cref="List{T}.Sort(IComparer{T})" />
        public void Sort(IComparer<T> comparer)
        {
            using (LockObject.EnterWriteLock())
                _lstData.Sort(comparer);
        }

        /// <inheritdoc cref="List{T}.Sort(int, int, IComparer{T})" />
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            using (LockObject.EnterWriteLock())
                _lstData.Sort(index, count, comparer);
        }

        /// <inheritdoc cref="List{T}.Sort(Comparison{T})" />
        public void Sort(Comparison<T> comparison)
        {
            using (LockObject.EnterWriteLock())
                _lstData.Sort(comparison);
        }

        /// <inheritdoc cref="List{T}.Sort()" />
        public async ValueTask SortAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                _lstData.Sort();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.Sort(IComparer{T})" />
        public async ValueTask SortAsync(IComparer<T> comparer)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                _lstData.Sort(comparer);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.Sort(int, int, IComparer{T})" />
        public async ValueTask SortAsync(int index, int count, IComparer<T> comparer)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                _lstData.Sort(index, count, comparer);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.Sort(Comparison{T})" />
        public async ValueTask SortAsync(Comparison<T> comparison)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                _lstData.Sort(comparison);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public T[] ToArray()
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.ToArray();
        }

        public async ValueTask<T[]> ToArrayAsync()
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.ToArray();
        }

        /// <inheritdoc cref="List{T}.TrimExcess" />
        public void TrimExcess()
        {
            using (LockObject.EnterWriteLock())
                _lstData.TrimExcess();
        }

        /// <inheritdoc cref="List{T}.TrimExcess" />
        public async ValueTask TrimExcessAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                _lstData.TrimExcess();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.TrueForAll" />
        public bool TrueForAll(Predicate<T> match)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.TrueForAll(match);
        }

        /// <inheritdoc cref="List{T}.TrueForAll" />
        public async ValueTask<bool> TrueForAllAsync(Predicate<T> match)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.TrueForAll(match);
        }

        /// <inheritdoc cref="List{T}.TrueForAll" />
        public async ValueTask<bool> TrueForAllAsync(Task<Predicate<T>> match)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.TrueForAll(await match);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                LockObject.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (disposing)
            {
                await LockObject.DisposeAsync();
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true);
            GC.SuppressFinalize(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
