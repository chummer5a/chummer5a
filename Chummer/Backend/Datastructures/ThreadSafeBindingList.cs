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
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    /// <summary>
    /// Thread-safe-wrapped version of CachedBindingList, but also with constraints on the generic type so that it can only be used on a class with INotifyPropertyChanged.
    /// Use ThreadSafeObservableCollection instead for classes without INotifyPropertyChanged.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ThreadSafeBindingList<T> : IAsyncList<T>, IAsyncReadOnlyList<T>, IBindingList, ICancelAddNew, IRaiseItemChangedEvents, IHasLockObject, IAsyncProducerConsumerCollection<T>, IAsyncEnumerableWithSideEffects<T> where T : INotifyPropertyChanged
    {
        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; }

        private readonly CachedBindingList<T> _lstData;

        public ThreadSafeBindingList(AsyncFriendlyReaderWriterLock objParentLock = null, bool blnLockReadOnlyForParent = false)
        {
            LockObject = new AsyncFriendlyReaderWriterLock(objParentLock, blnLockReadOnlyForParent);
            _lstData = new CachedBindingList<T>
            {
                BindingListLock = LockObject
            };
        }

        public ThreadSafeBindingList(IList<T> list, AsyncFriendlyReaderWriterLock objParentLock = null, bool blnLockReadOnlyForParent = false)
        {
            LockObject = new AsyncFriendlyReaderWriterLock(objParentLock, blnLockReadOnlyForParent);
            _lstData = new CachedBindingList<T>(list)
            {
                BindingListLock = LockObject
            };
        }

        /// <inheritdoc cref="List{T}.Count" />
        public int Count
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstData.Count;
            }
        }

        public async Task<int> GetCountAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await _lstData.GetCountAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
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
                using (LockObject.EnterReadLock())
                    return _lstData[index];
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_lstData[index].Equals(value))
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_lstData[index].Equals(value))
                        return;
                    using (LockObject.EnterWriteLock())
                        _lstData[index] = value;
                }
            }
        }

        public async Task<T> GetValueAtAsync(int index, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await _lstData.GetValueAtAsync(index, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetValueAtAsync(int index, T value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_lstData[index].Equals(value))
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_lstData[index].Equals(value))
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    await _lstData.SetValueAtAsync(index, value, token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        object IList.this[int index]
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstData[index];
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_lstData[index].Equals(value))
                        return;
                }
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_lstData[index].Equals(value))
                        return;
                    using (LockObject.EnterWriteLock())
                        _lstData[index] = (T)value;
                }
            }
        }

        public bool SequenceEqual(ThreadSafeBindingList<T> other)
        {
            if (other == null)
                return false;
            using (other.LockObject.EnterReadLock())
            using (LockObject.EnterReadLock())
            {
                return _lstData.Count == other._lstData.Count
                       && _lstData.SequenceEqual(other._lstData);
            }
        }

        public async Task<bool> SequenceEqualAsync(ThreadSafeBindingList<T> other, CancellationToken token = default)
        {
            if (other == null)
                return false;
            IAsyncDisposable objLocker = await other.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                IAsyncDisposable objLocker2 = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                try
                {
                    return _lstData.Count == other._lstData.Count
                           && _lstData.SequenceEqual(other._lstData);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public int Add(object value)
        {
            if (value is not T objValue)
                return -1;
            using (LockObject.EnterUpgradeableReadLock())
            {
                Add(objValue);
                return _lstData.Count - 1;
            }
        }

        /// <inheritdoc />
        public void Add(T item)
        {
            using (LockObject.EnterWriteLock())
                _lstData.Add(item);
        }

        public async Task AddAsync(T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                await _lstData.AddAsync(item, token).ConfigureAwait(false);
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

        public async Task AddRangeAsync(IEnumerable<T> collection, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _lstData.AddRange(collection);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.BinarySearch(int, int, T, IComparer{T})" />
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            using (LockObject.EnterReadLock())
                return _lstData.BinarySearch(index, count, item, comparer);
        }

        /// <inheritdoc cref="List{T}.BinarySearch(T, IComparer{T})" />
        public int BinarySearch(T item, IComparer<T> comparer)
        {
            using (LockObject.EnterReadLock())
                return _lstData.BinarySearch(item, comparer);
        }

        /// <inheritdoc cref="List{T}.BinarySearch(int, int, T, IComparer{T})" />
        public async Task<int> BinarySearchAsync(int index, int count, T item, IComparer<T> comparer, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.BinarySearch(index, count, item, comparer);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.BinarySearch(T, IComparer{T})" />
        public async Task<int> BinarySearchAsync(T item, IComparer<T> comparer, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.BinarySearch(item, comparer);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.Clear" />
        public void Clear()
        {
            using (LockObject.EnterWriteLock())
                _lstData.Clear();
        }

        /// <inheritdoc cref="List{T}.Clear" />
        public async Task ClearAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                await _lstData.ClearAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool Contains(object value)
        {
            if (!(value is T objValue))
                return false;
            return Contains(objValue);
        }

        /// <inheritdoc cref="List{T}.Contains" />
        public bool Contains(T item)
        {
            using (LockObject.EnterReadLock())
                return _lstData.Contains(item);
        }

        /// <inheritdoc cref="List{T}.Contains" />
        public async Task<bool> ContainsAsync(T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await _lstData.ContainsAsync(item, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public void CopyTo(Array array, int index)
        {
            using (LockObject.EnterReadLock())
            {
                foreach (T objItem in _lstData)
                {
                    array.SetValue(objItem, index);
                    ++index;
                }
            }
        }

        public async Task CopyToAsync(Array array, int index, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                foreach (T objItem in _lstData)
                {
                    array.SetValue(objItem, index);
                    ++index;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.CopyTo(T[], int)" />
        public void CopyTo(T[] array, int arrayIndex)
        {
            using (LockObject.EnterReadLock())
                _lstData.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc cref="List{T}.CopyTo(T[], int)" />
        public async Task CopyToAsync(T[] array, int index, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                await _lstData.CopyToAsync(array, index, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public bool TryAdd(T item)
        {
            Add(item);
            return true;
        }

        public async Task<bool> TryAddAsync(T item, CancellationToken token = default)
        {
            await AddAsync(item, token).ConfigureAwait(false);
            return true;
        }

        /// <inheritdoc />
        public async Task<Tuple<bool, T>> TryTakeAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_lstData.Count == 0)
                    return new Tuple<bool, T>(false, default);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (await _lstData.GetCountAsync(token).ConfigureAwait(false) > 0)
                {
                    // FIFO to be compliant with how the default for BlockingCollection<T> is ConcurrentQueue
                    T objReturn = await _lstData.GetValueAtAsync(0, token).ConfigureAwait(false);
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        await _lstData.RemoveAtAsync(0, token).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }

                    return new Tuple<bool, T>(true, objReturn);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            return new Tuple<bool, T>(false, default);
        }

        /// <inheritdoc />
        public bool TryTake(out T item)
        {
            using (LockObject.EnterReadLock())
            {
                if (_lstData.Count == 0)
                {
                    item = default;
                    return false;
                }
            }
            using (LockObject.EnterUpgradeableReadLock())
            {
                if (_lstData.Count > 0)
                {
                    // FIFO to be compliant with how the default for BlockingCollection<T> is ConcurrentQueue
                    item = _lstData[0];
                    using (LockObject.EnterWriteLock())
                    {
                        _lstData.RemoveAt(0);
                    }

                    return true;
                }
            }

            item = default;
            return false;
        }

        /// <inheritdoc cref="List{T}.Exists" />
        public bool Exists(Predicate<T> match)
        {
            using (LockObject.EnterReadLock())
                return _lstData.Exists(match);
        }

        /// <inheritdoc cref="List{T}.Exists" />
        public async Task<bool> ExistsAsync(Predicate<T> match, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.Exists(match);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.Find" />
        public T Find(Predicate<T> match)
        {
            using (LockObject.EnterReadLock())
                return _lstData.Find(match);
        }

        /// <inheritdoc cref="List{T}.Find" />
        public async Task<T> FindAsync(Predicate<T> match, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.Find(match);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.FindAll" />
        public List<T> FindAll(Predicate<T> match)
        {
            using (LockObject.EnterReadLock())
                return _lstData.FindAll(match);
        }

        /// <inheritdoc cref="List{T}.FindAll" />
        public async Task<List<T>> FindAllAsync(Predicate<T> match, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.FindAll(match);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.FindIndex(Predicate{T})" />
        public int FindIndex(Predicate<T> match)
        {
            using (LockObject.EnterReadLock())
                return _lstData.FindIndex(match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(int, Predicate{T})" />
        public int FindIndex(int startIndex, Predicate<T> match)
        {
            using (LockObject.EnterReadLock())
                return _lstData.FindIndex(startIndex, match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(int, int, Predicate{T})" />
        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            using (LockObject.EnterReadLock())
                return _lstData.FindIndex(startIndex, count, match);
        }

        /// <inheritdoc cref="List{T}.FindIndex(Predicate{T})" />
        public async Task<int> FindIndexAsync(Predicate<T> match, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await _lstData.FindIndexAsync(match, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.FindIndex(int, Predicate{T})" />
        public async Task<int> FindIndexAsync(int startIndex, Predicate<T> match, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await _lstData.FindIndexAsync(startIndex, match, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.FindIndex(int, int, Predicate{T})" />
        public async Task<int> FindIndexAsync(int startIndex, int count, Predicate<T> match, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await _lstData.FindIndexAsync(startIndex, count, match, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.FindLast" />
        public T FindLast(Predicate<T> match)
        {
            using (LockObject.EnterReadLock())
                return _lstData.FindLast(match);
        }

        /// <inheritdoc cref="List{T}.FindLast" />
        public async Task<T> FindLastAsync(Predicate<T> match, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.FindLast(match);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(Predicate{T})" />
        public int FindLastIndex(Predicate<T> match)
        {
            using (LockObject.EnterReadLock())
                return _lstData.FindIndex(match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(int, Predicate{T})" />
        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            using (LockObject.EnterReadLock())
                return _lstData.FindIndex(startIndex, match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(int, int, Predicate{T})" />
        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            using (LockObject.EnterReadLock())
                return _lstData.FindLastIndex(startIndex, count, match);
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(Predicate{T})" />
        public async Task<int> FindLastIndexAsync(Predicate<T> match, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await _lstData.FindLastIndexAsync(match, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(int, Predicate{T})" />
        public async Task<int> FindLastIndexAsync(int startIndex, Predicate<T> match, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await _lstData.FindLastIndexAsync(startIndex, match, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.FindLastIndex(int, int, Predicate{T})" />
        public async Task<int> FindLastIndexAsync(int startIndex, int count, Predicate<T> match, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await _lstData.FindLastIndexAsync(startIndex, count, match, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.GetEnumerator" />
        public IEnumerator<T> GetEnumerator()
        {
            LockingEnumerator<T> objReturn = LockingEnumerator<T>.Get(this);
            objReturn.SetEnumerator(_lstData.GetEnumerator());
            return objReturn;
        }

        public Task<IEnumerator<T>> GetEnumeratorAsync(CancellationToken token = default)
        {
            // Needs to be like this (using async inner function) to make sure AsyncLocals for parents are set in proper location
            Task<LockingEnumerator<T>> tskReturn = LockingEnumerator<T>.GetAsync(this, token);
            return Inner(tskReturn);
            async Task<IEnumerator<T>> Inner(Task<LockingEnumerator<T>> tskInner)
            {
                LockingEnumerator<T> objResult = await tskInner.ConfigureAwait(false);
                objResult.SetEnumerator(_lstData.GetEnumerator());
                return objResult;
            }
        }

        public IEnumerator<T> EnumerateWithSideEffects()
        {
            LockingEnumerator<T> objReturn = LockingEnumerator<T>.GetWithSideEffects(this);
            objReturn.SetEnumerator(_lstData.GetEnumerator());
            return objReturn;
        }

        public Task<IEnumerator<T>> EnumerateWithSideEffectsAsync(CancellationToken token = default)
        {
            // Needs to be like this (using async inner function) to make sure AsyncLocals for parents are set in proper location
            Task<LockingEnumerator<T>> tskReturn = LockingEnumerator<T>.GetWithSideEffectsAsync(this, token);
            return Inner(tskReturn);
            async Task<IEnumerator<T>> Inner(Task<LockingEnumerator<T>> tskInner)
            {
                LockingEnumerator<T> objResult = await tskInner.ConfigureAwait(false);
                objResult.SetEnumerator(_lstData.GetEnumerator());
                return objResult;
            }
        }

        public int IndexOf(object value)
        {
            if (!(value is T objValue))
                return -1;
            return IndexOf(objValue);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T)" />
        public int IndexOf(T item)
        {
            using (LockObject.EnterReadLock())
                return _lstData.IndexOf(item);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T)" />
        public async Task<int> IndexOfAsync(T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await _lstData.IndexOfAsync(item, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public void Insert(int index, object value)
        {
            if (!(value is T objValue))
                return;
            Insert(index, objValue);
        }

        /// <inheritdoc cref="List{T}.Insert" />
        public void Insert(int index, T item)
        {
            using (LockObject.EnterWriteLock())
                _lstData.Insert(index, item);
        }

        /// <inheritdoc cref="List{T}.Insert" />
        public async Task InsertAsync(int index, T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                await _lstData.InsertAsync(index, item, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T)" />
        public int LastIndexOf(T item)
        {
            using (LockObject.EnterReadLock())
                return _lstData.LastIndexOf(item);
        }

        /// <inheritdoc cref="List{T}.LastIndexOf(T)" />
        public async Task<int> LastIndexOfAsync(T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await _lstData.LastIndexOfAsync(item, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public void Remove(object value)
        {
            if (!(value is T objValue))
                return;
            Remove(objValue);
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            using (LockObject.EnterWriteLock())
                return _lstData.Remove(item);
        }

        public async Task<bool> RemoveAsync(T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await _lstData.RemoveAsync(item, token).ConfigureAwait(false);
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
        public async Task RemoveAtAsync(int index, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                await _lstData.RemoveAtAsync(index, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public T[] ToArray()
        {
            using (LockObject.EnterReadLock())
                return _lstData.ToArray();
        }

        public async Task<T[]> ToArrayAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.ToArray();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private int _intIsDisposed;

        public bool IsDisposed => _intIsDisposed > 0;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Interlocked.CompareExchange(ref _intIsDisposed, 1, 0) > 0)
                    return;
                using (LockObject.EnterWriteLock())
                    _lstData.Dispose();
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
                if (Interlocked.CompareExchange(ref _intIsDisposed, 1, 0) > 0)
                    return;
                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
                try
                {
                    await _lstData.DisposeAsync().ConfigureAwait(false);
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
                await LockObject.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true).ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public event EventHandler<RemovingOldEventArgs> BeforeRemove
        {
            add
            {
                using (LockObject.EnterWriteLock())
                    _lstData.BeforeRemove += value;
            }
            remove
            {
                using (LockObject.EnterWriteLock())
                    _lstData.BeforeRemove -= value;
            }
        }

        public event AsyncBeforeRemoveEventHandler BeforeRemoveAsync
        {
            add => _lstData.BeforeRemoveAsync += value;
            remove => _lstData.BeforeRemoveAsync -= value;
        }

        /// <inheritdoc cref="BindingList{T}.AddingNew" />
        public event AddingNewEventHandler AddingNew
        {
            add
            {
                using (LockObject.EnterWriteLock())
                    _lstData.AddingNew += value;
            }
            remove
            {
                using (LockObject.EnterWriteLock())
                    _lstData.AddingNew -= value;
            }
        }

        public event AsyncAddingNewEventHandler AddingNewAsync
        {
            add => _lstData.AddingNewAsync += value;
            remove => _lstData.AddingNewAsync -= value;
        }

        /// <inheritdoc />
        public ListSortDirection SortDirection => ListSortDirection.Ascending;

        /// <inheritdoc cref="BindingList{T}.ListChanged" />
        public event ListChangedEventHandler ListChanged
        {
            add
            {
                using (LockObject.EnterWriteLock())
                    _lstData.ListChanged += value;
            }
            remove
            {
                using (LockObject.EnterWriteLock())
                    _lstData.ListChanged -= value;
            }
        }

        public event AsyncListChangedEventHandler ListChangedAsync
        {
            add => _lstData.ListChangedAsync += value;
            remove => _lstData.ListChangedAsync -= value;
        }

        /// <inheritdoc cref="BindingList{T}.RaiseListChangedEvents" />
        public bool RaiseListChangedEvents
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstData.RaiseListChangedEvents;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_lstData.RaiseListChangedEvents.Equals(value))
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_lstData.RaiseListChangedEvents.Equals(value))
                        return;
                    using (LockObject.EnterWriteLock())
                        _lstData.RaiseListChangedEvents = value;
                }
            }
        }

        /// <inheritdoc />
        public void AddIndex(PropertyDescriptor property)
        {
            //do nothing
        }

        /// <inheritdoc />
        public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public int Find(PropertyDescriptor property, object key)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public void RemoveIndex(PropertyDescriptor property)
        {
            //do nothing
        }

        /// <inheritdoc />
        public void RemoveSort()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc cref="BindingList{T}.AllowNew" />
        public bool AllowNew
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstData.AllowNew;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_lstData.AllowNew.Equals(value))
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_lstData.AllowNew.Equals(value))
                        return;
                    using (LockObject.EnterWriteLock())
                        _lstData.AllowNew = value;
                }
            }
        }

        /// <inheritdoc cref="BindingList{T}.AllowEdit" />
        public bool AllowEdit
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstData.AllowEdit;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_lstData.AllowEdit.Equals(value))
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_lstData.AllowEdit.Equals(value))
                        return;
                    using (LockObject.EnterWriteLock())
                        _lstData.AllowEdit = value;
                }
            }
        }

        /// <inheritdoc cref="BindingList{T}.AllowRemove" />
        public bool AllowRemove
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstData.AllowRemove;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_lstData.AllowRemove.Equals(value))
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_lstData.AllowRemove.Equals(value))
                        return;
                    using (LockObject.EnterWriteLock())
                        _lstData.AllowRemove = value;
                }
            }
        }

        /// <inheritdoc />
        public bool SupportsChangeNotification => true;

        /// <inheritdoc />
        public bool SupportsSearching => false;

        /// <inheritdoc />
        public bool SupportsSorting => false;

        /// <inheritdoc />
        public bool IsSorted => false;

        /// <inheritdoc />
        public PropertyDescriptor SortProperty => null;

        /// <inheritdoc />
        public object AddNew()
        {
            using (LockObject.EnterWriteLock())
                return _lstData.AddNew();
        }

        /// <inheritdoc cref="BindingList{T}.AddNew()" />
        public async Task<T> AddNewAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstData.AddNew();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public void CancelNew(int itemIndex)
        {
            using (LockObject.EnterWriteLock())
                _lstData.CancelNew(itemIndex);
        }

        /// <inheritdoc />
        public void EndNew(int itemIndex)
        {
            using (LockObject.EnterWriteLock())
                _lstData.EndNew(itemIndex);
        }

        /// <inheritdoc cref="BindingList{T}.ResetBindings" />
        public void ResetBindings()
        {
            using (LockObject.EnterWriteLock())
                _lstData.ResetBindings();
        }

        /// <inheritdoc cref="BindingList{T}.ResetBindings" />
        public async Task ResetBindingsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                await _lstData.ResetBindingsAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="BindingList{T}.ResetItem" />
        public void ResetItem(int position)
        {
            using (LockObject.EnterWriteLock())
                _lstData.ResetItem(position);
        }

        /// <inheritdoc cref="BindingList{T}.ResetItem" />
        public async Task ResetItemAsync(int position, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                await _lstData.ResetItemAsync(position, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public bool RaisesItemChangedEvents => true;

        /// <summary>
        /// Sorts the elements in a range of elements in an ObservableCollection using the specified
        /// System.Collections.Generic.IComparer`1 generic interface.
        /// </summary>
        /// <param name="index">The starting index of the range to sort.</param>
        /// <param name="length">The number of elements in the range to sort.</param>
        /// <param name="objComparer">The System.Collections.Generic.IComparer`1 generic interface
        /// implementation to use when comparing elements, or null to use the System.IComparable`1 generic
        /// interface implementation of each element.</param>
        public void Sort(int index, int length, IComparer<T> objComparer = null)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (length == 0)
                return;
            if (index + length > Count)
                throw new InvalidOperationException(nameof(length));
            using (LockObject.EnterUpgradeableReadLock())
            {
                if (index + length > _lstData.Count)
                    throw new InvalidOperationException(nameof(length));
                IDisposable[] aobjLockers = _lstData[index] is IHasLockObject ? new IDisposable[length] : null;
                T[] aobjSorted = new T[length];
                for (int i = 0; i < length; ++i)
                {
                    T objLoop = _lstData[index + i];
                    aobjSorted[i] = objLoop;
                    if (aobjLockers != null)
                        aobjLockers[i] = (objLoop as IHasLockObject)?.LockObject.EnterReadLock();
                }

                Array.Sort(aobjSorted, objComparer);

                if (aobjLockers != null)
                {
                    foreach (IDisposable objLocker in aobjLockers)
                    {
                        objLocker.Dispose();
                    }
                }

                if (!_lstData.RaiseListChangedEvents)
                {
                    using (LockObject.EnterWriteLock())
                    {
                        for (int i = 0; i < aobjSorted.Length; ++i)
                        {
                            _lstData[index + i] = aobjSorted[i];
                        }
                    }
                    return;
                }
                // If at least half of the list was changed, call a reset event instead of a large amount of ItemChanged events
                int intResetThreshold = aobjSorted.Length / 2;
                int intCountChanges = 0;
                // Not BitArray because read/write performance is much more important here than memory footprint
                bool[] ablnItemChanged = new bool[aobjSorted.Length];
                using (LockObject.EnterWriteLock())
                {
                    // We're going to disable events while we work with the list, then call them all at once at the end
                    _lstData.RaiseListChangedEvents = false;
                    try
                    {
                        for (int i = 0; i < aobjSorted.Length; ++i)
                        {
                            T objLoop = aobjSorted[i];
                            if (ReferenceEquals(objLoop, _lstData[index + i]))
                                continue;
                            ablnItemChanged[i] = true;
                            ++intCountChanges;
                            _lstData[index + i] = objLoop;
                        }
                    }
                    finally
                    {
                        _lstData.RaiseListChangedEvents = true;
                    }

                    if (intCountChanges >= intResetThreshold)
                    {
                        _lstData.ResetBindings();
                    }
                    else
                    {
                        for (int i = 0; i < ablnItemChanged.Length; ++i)
                        {
                            if (ablnItemChanged[i])
                                _lstData.ResetItem(index + i);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sorts the elements in a range of elements in an ObservableCollection using the specified System.Comparison`1.
        /// </summary>
        /// <param name="funcComparison">The System.Comparison`1 to use when comparing elements.</param>
        public void Sort(Comparison<T> funcComparison)
        {
            if (funcComparison == null)
                throw new ArgumentNullException(nameof(funcComparison));
            if (Count == 0)
                return;
            using (LockObject.EnterUpgradeableReadLock())
            {
                if (_lstData.Count == 0)
                    return;
                IDisposable[] aobjLockers = _lstData[0] is IHasLockObject ? new IDisposable[_lstData.Count] : null;
                T[] aobjSorted = new T[_lstData.Count];
                for (int i = 0; i < _lstData.Count; ++i)
                {
                    T objLoop = _lstData[i];
                    aobjSorted[i] = objLoop;
                    if (aobjLockers != null)
                        aobjLockers[i] = (objLoop as IHasLockObject)?.LockObject.EnterReadLock();
                }

                Array.Sort(aobjSorted, funcComparison);

                if (aobjLockers != null)
                {
                    foreach (IDisposable objLocker in aobjLockers)
                    {
                        objLocker.Dispose();
                    }
                }

                if (!_lstData.RaiseListChangedEvents)
                {
                    using (LockObject.EnterWriteLock())
                    {
                        for (int i = 0; i < aobjSorted.Length; ++i)
                        {
                            _lstData[i] = aobjSorted[i];
                        }
                    }
                    return;
                }
                // If at least half of the list was changed, call a reset event instead of a large amount of ItemChanged events
                int intResetThreshold = aobjSorted.Length / 2;
                int intCountChanges = 0;
                // Not BitArray because read/write performance is much more important here than memory footprint
                bool[] ablnItemChanged = new bool[aobjSorted.Length];
                using (LockObject.EnterWriteLock())
                {
                    // We're going to disable events while we work with the list, then call them all at once at the end
                    _lstData.RaiseListChangedEvents = false;
                    try
                    {
                        for (int i = 0; i < aobjSorted.Length; ++i)
                        {
                            T objLoop = aobjSorted[i];
                            if (ReferenceEquals(objLoop, _lstData[i]))
                                continue;
                            ablnItemChanged[i] = true;
                            ++intCountChanges;
                            _lstData[i] = objLoop;
                        }
                    }
                    finally
                    {
                        _lstData.RaiseListChangedEvents = true;
                    }

                    if (intCountChanges >= intResetThreshold)
                    {
                        _lstData.ResetBindings();
                    }
                    else
                    {
                        for (int i = 0; i < ablnItemChanged.Length; ++i)
                        {
                            if (ablnItemChanged[i])
                                _lstData.ResetItem(i);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sorts the elements in a range of elements in an ObservableCollection using the specified
        /// System.Collections.Generic.IComparer`1 generic interface.
        /// </summary>
        /// <param name="objComparer">The System.Collections.Generic.IComparer`1 generic interface
        /// implementation to use when comparing elements, or null to use the System.IComparable`1 generic
        /// interface implementation of each element.</param>
        public void Sort(IComparer<T> objComparer = null)
        {
            if (Count == 0)
                return;
            using (LockObject.EnterUpgradeableReadLock())
            {
                if (_lstData.Count == 0)
                    return;
                IDisposable[] aobjLockers = _lstData[0] is IHasLockObject ? new IDisposable[_lstData.Count] : null;
                T[] aobjSorted = new T[_lstData.Count];
                for (int i = 0; i < _lstData.Count; ++i)
                {
                    T objLoop = _lstData[i];
                    aobjSorted[i] = objLoop;
                    if (aobjLockers != null)
                        aobjLockers[i] = (objLoop as IHasLockObject)?.LockObject.EnterReadLock();
                }

                Array.Sort(aobjSorted, objComparer);

                if (aobjLockers != null)
                {
                    foreach (IDisposable objLocker in aobjLockers)
                    {
                        objLocker.Dispose();
                    }
                }

                if (!_lstData.RaiseListChangedEvents)
                {
                    using (LockObject.EnterWriteLock())
                    {
                        for (int i = 0; i < aobjSorted.Length; ++i)
                        {
                            _lstData[i] = aobjSorted[i];
                        }
                    }
                    return;
                }
                // If at least half of the list was changed, call a reset event instead of a large amount of ItemChanged events
                int intResetThreshold = aobjSorted.Length / 2;
                int intCountChanges = 0;
                // Not BitArray because read/write performance is much more important here than memory footprint
                bool[] ablnItemChanged = new bool[aobjSorted.Length];
                using (LockObject.EnterWriteLock())
                {
                    // We're going to disable events while we work with the list, then call them all at once at the end
                    _lstData.RaiseListChangedEvents = false;
                    try
                    {
                        for (int i = 0; i < aobjSorted.Length; ++i)
                        {
                            T objLoop = aobjSorted[i];
                            if (ReferenceEquals(objLoop, _lstData[i]))
                                continue;
                            ablnItemChanged[i] = true;
                            ++intCountChanges;
                            _lstData[i] = objLoop;
                        }
                    }
                    finally
                    {
                        _lstData.RaiseListChangedEvents = true;
                    }

                    if (intCountChanges >= intResetThreshold)
                    {
                        _lstData.ResetBindings();
                    }
                    else
                    {
                        for (int i = 0; i < ablnItemChanged.Length; ++i)
                        {
                            if (ablnItemChanged[i])
                                _lstData.ResetItem(i);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sorts the elements in a range of elements in an ObservableCollection using the specified
        /// System.Collections.Generic.IComparer`1 generic interface.
        /// </summary>
        /// <param name="index">The starting index of the range to sort.</param>
        /// <param name="length">The number of elements in the range to sort.</param>
        /// <param name="objComparer">The System.Collections.Generic.IComparer`1 generic interface
        /// implementation to use when comparing elements, or null to use the System.IComparable`1 generic
        /// interface implementation of each element.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task SortAsync(int index, int length, IComparer<T> objComparer = null, CancellationToken token = default)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (index + length > await GetCountAsync(token).ConfigureAwait(false))
                throw new InvalidOperationException(nameof(length));
            if (length == 0)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Stack<IAsyncDisposable> stkLockers = _lstData[0] is IHasLockObject ? new Stack<IAsyncDisposable>(length) : null;
                T[] aobjSorted = new T[length];
                try
                {
                    for (int i = 0; i < length; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objLoop = _lstData[index + i];
                        aobjSorted[i] = objLoop;
                        if (stkLockers != null && objLoop is IHasLockObject objLoopWithLocker)
                            stkLockers.Push(await objLoopWithLocker.LockObject.EnterReadLockAsync(token)
                                .ConfigureAwait(false));
                    }

                    token.ThrowIfCancellationRequested();
                    Array.Sort(aobjSorted, objComparer);
                }
                finally
                {
                    if (stkLockers != null)
                    {
                        while (stkLockers.Count > 0)
                        {
                            IAsyncDisposable objLocker2 = stkLockers.Pop();
                            if (objLocker2 != null)
                                await objLocker2.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }

                token.ThrowIfCancellationRequested();
                if (!_lstData.RaiseListChangedEvents)
                {
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        for (int i = 0; i < aobjSorted.Length; ++i)
                        {
                            _lstData[index + i] = aobjSorted[i];
                        }
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }

                    return;
                }

                // If at least half of the list was changed, call a reset event instead of a large amount of ItemChanged events
                int intResetThreshold = aobjSorted.Length / 2;
                int intCountChanges = 0;
                // Not BitArray because read/write performance is much more important here than memory footprint
                bool[] ablnItemChanged = new bool[aobjSorted.Length];
                IAsyncDisposable objLocker3 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    // We're going to disable events while we work with the list, then call them all at once at the end
                    _lstData.RaiseListChangedEvents = false;
                    try
                    {
                        for (int i = 0; i < aobjSorted.Length; ++i)
                        {
                            T objLoop = aobjSorted[i];
                            if (ReferenceEquals(objLoop, _lstData[index + i]))
                                continue;
                            ablnItemChanged[i] = true;
                            ++intCountChanges;
                            _lstData[index + i] = objLoop;
                        }
                    }
                    finally
                    {
                        _lstData.RaiseListChangedEvents = true;
                    }

                    if (intCountChanges >= intResetThreshold)
                    {
                        await _lstData.ResetBindingsAsync(token).ConfigureAwait(false);
                    }
                    else
                    {
                        for (int i = 0; i < ablnItemChanged.Length; ++i)
                        {
                            if (ablnItemChanged[i])
                                await _lstData.ResetItemAsync(index + i, token).ConfigureAwait(false);
                        }
                    }
                }
                finally
                {
                    await objLocker3.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Sorts the elements in a range of elements in an ObservableCollection using the specified System.Comparison`1.
        /// </summary>
        /// <param name="funcComparison">The System.Comparison`1 to use when comparing elements.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task SortAsync(Comparison<T> funcComparison, CancellationToken token = default)
        {
            if (funcComparison == null)
                throw new ArgumentNullException(nameof(funcComparison));
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_lstData.Count == 0)
                    return;
                Stack<IAsyncDisposable> stkLockers =
                    _lstData[0] is IHasLockObject ? new Stack<IAsyncDisposable>(_lstData.Count) : null;
                T[] aobjSorted = new T[_lstData.Count];
                try
                {
                    for (int i = 0; i < _lstData.Count; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objLoop = _lstData[i];
                        aobjSorted[i] = objLoop;
                        if (stkLockers != null && objLoop is IHasLockObject objLoopWithLocker)
                            stkLockers.Push(await objLoopWithLocker.LockObject.EnterReadLockAsync(token)
                                .ConfigureAwait(false));
                    }

                    token.ThrowIfCancellationRequested();
                    Array.Sort(aobjSorted, funcComparison);
                }
                finally
                {
                    if (stkLockers != null)
                    {
                        while (stkLockers.Count > 0)
                        {
                            IAsyncDisposable objLocker3 = stkLockers.Pop();
                            if (objLocker3 != null)
                                await objLocker3.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }

                token.ThrowIfCancellationRequested();
                if (!_lstData.RaiseListChangedEvents)
                {
                    IAsyncDisposable objLocker3 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        for (int i = 0; i < aobjSorted.Length; ++i)
                        {
                            _lstData[i] = aobjSorted[i];
                        }
                    }
                    finally
                    {
                        await objLocker3.DisposeAsync().ConfigureAwait(false);
                    }

                    return;
                }

                // If at least half of the list was changed, call a reset event instead of a large amount of ItemChanged events
                int intResetThreshold = aobjSorted.Length / 2;
                int intCountChanges = 0;
                // Not BitArray because read/write performance is much more important here than memory footprint
                bool[] ablnItemChanged = new bool[aobjSorted.Length];
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    // We're going to disable events while we work with the list, then call them all at once at the end
                    _lstData.RaiseListChangedEvents = false;
                    try
                    {
                        for (int i = 0; i < aobjSorted.Length; ++i)
                        {
                            T objLoop = aobjSorted[i];
                            if (ReferenceEquals(objLoop, _lstData[i]))
                                continue;
                            ablnItemChanged[i] = true;
                            ++intCountChanges;
                            _lstData[i] = objLoop;
                        }
                    }
                    finally
                    {
                        _lstData.RaiseListChangedEvents = true;
                    }

                    if (intCountChanges >= intResetThreshold)
                    {
                        await _lstData.ResetBindingsAsync(token).ConfigureAwait(false);
                    }
                    else
                    {
                        for (int i = 0; i < ablnItemChanged.Length; ++i)
                        {
                            if (ablnItemChanged[i])
                                await _lstData.ResetItemAsync(i, token).ConfigureAwait(false);
                        }
                    }
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Sorts the elements in a range of elements in an ObservableCollection using the specified
        /// System.Collections.Generic.IComparer`1 generic interface.
        /// </summary>
        /// <param name="objComparer">The System.Collections.Generic.IComparer`1 generic interface
        /// implementation to use when comparing elements, or null to use the System.IComparable`1 generic
        /// interface implementation of each element.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task SortAsync(IComparer<T> objComparer = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (await GetCountAsync(token).ConfigureAwait(false) == 0)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_lstData.Count == 0)
                    return;
                Stack<IAsyncDisposable> stkLockers =
                    _lstData[0] is IHasLockObject ? new Stack<IAsyncDisposable>(_lstData.Count) : null;
                T[] aobjSorted = new T[_lstData.Count];
                try
                {
                    for (int i = 0; i < _lstData.Count; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objLoop = _lstData[i];
                        aobjSorted[i] = objLoop;
                        if (stkLockers != null && objLoop is IHasLockObject objLoopWithLocker)
                            stkLockers.Push(await objLoopWithLocker.LockObject.EnterReadLockAsync(token)
                                .ConfigureAwait(false));
                    }

                    token.ThrowIfCancellationRequested();
                    Array.Sort(aobjSorted, objComparer);
                }
                finally
                {
                    if (stkLockers != null)
                    {
                        while (stkLockers.Count > 0)
                        {
                            IAsyncDisposable objLocker2 = stkLockers.Pop();
                            if (objLocker2 != null)
                                await objLocker2.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }

                token.ThrowIfCancellationRequested();
                if (!_lstData.RaiseListChangedEvents)
                {
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        for (int i = 0; i < aobjSorted.Length; ++i)
                        {
                            _lstData[i] = aobjSorted[i];
                        }
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }

                    return;
                }

                // If at least half of the list was changed, call a reset event instead of a large amount of ItemChanged events
                int intResetThreshold = aobjSorted.Length / 2;
                int intCountChanges = 0;
                // Not BitArray because read/write performance is much more important here than memory footprint
                bool[] ablnItemChanged = new bool[aobjSorted.Length];
                IAsyncDisposable objLocker3 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    // We're going to disable events while we work with the list, then call them all at once at the end
                    _lstData.RaiseListChangedEvents = false;
                    try
                    {
                        for (int i = 0; i < aobjSorted.Length; ++i)
                        {
                            T objLoop = aobjSorted[i];
                            if (ReferenceEquals(objLoop, _lstData[i]))
                                continue;
                            ablnItemChanged[i] = true;
                            ++intCountChanges;
                            _lstData[i] = objLoop;
                        }
                    }
                    finally
                    {
                        _lstData.RaiseListChangedEvents = true;
                    }

                    if (intCountChanges >= intResetThreshold)
                    {
                        await _lstData.ResetBindingsAsync(token).ConfigureAwait(false);
                    }
                    else
                    {
                        for (int i = 0; i < ablnItemChanged.Length; ++i)
                        {
                            if (ablnItemChanged[i])
                                await _lstData.ResetItemAsync(i, token).ConfigureAwait(false);
                        }
                    }
                }
                finally
                {
                    await objLocker3.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public void Move(int intOldIndex, int intNewIndex, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (intOldIndex == intNewIndex)
                return;
            int intParity = intOldIndex < intNewIndex ? 1 : -1;
            using (LockObject.EnterUpgradeableReadLock(token))
            {
                using (LockObject.EnterWriteLock(token))
                {
                    bool blnOldRaiseListChangedEvents = _lstData.RaiseListChangedEvents;
                    try
                    {
                        _lstData.RaiseListChangedEvents = false;
                        for (int i = intOldIndex; i != intNewIndex; i += intParity)
                        {
                            token.ThrowIfCancellationRequested();
                            (_lstData[intOldIndex + intParity], _lstData[intOldIndex]) = (
                                _lstData[intOldIndex], _lstData[intOldIndex + intParity]);
                        }
                    }
                    finally
                    {
                        _lstData.RaiseListChangedEvents = blnOldRaiseListChangedEvents;
                    }
                }

                if (Math.Abs(intOldIndex - intNewIndex) >= _lstData.Count)
                {
                    _lstData.ResetBindings();
                }
                else
                {
                    for (int i = intOldIndex; i != intNewIndex; i += intParity)
                    {
                        _lstData.ResetItem(i);
                    }
                }
            }
        }

        public async Task MoveAsync(int intOldIndex, int intNewIndex, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (intOldIndex == intNewIndex)
                return;
            int intParity = intOldIndex < intNewIndex ? 1 : -1;
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    bool blnOldRaiseListChangedEvents = _lstData.RaiseListChangedEvents;
                    try
                    {
                        _lstData.RaiseListChangedEvents = false;
                        for (int i = intOldIndex; i != intNewIndex; i += intParity)
                        {
                            token.ThrowIfCancellationRequested();
                            (_lstData[intOldIndex + intParity], _lstData[intOldIndex]) = (
                                _lstData[intOldIndex], _lstData[intOldIndex + intParity]);
                        }
                    }
                    finally
                    {
                        _lstData.RaiseListChangedEvents = blnOldRaiseListChangedEvents;
                    }
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                if (Math.Abs(intOldIndex - intNewIndex) >= await _lstData.GetCountAsync(token).ConfigureAwait(false))
                {
                    await _lstData.ResetBindingsAsync(token).ConfigureAwait(false);
                }
                else
                {
                    for (int i = intOldIndex; i != intNewIndex; i += intParity)
                    {
                        await _lstData.ResetItemAsync(i, token).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
