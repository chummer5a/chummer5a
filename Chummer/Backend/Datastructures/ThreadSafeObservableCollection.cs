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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Chummer.Annotations;

namespace Chummer
{
    public class ThreadSafeObservableCollection<T> : IList<T>, IList, IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged, IProducerConsumerCollection<T>, IHasLockObject, IAsyncReadOnlyCollection<T>
    {
        private readonly EnhancedObservableCollection<T> _lstData;
        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();

        public ThreadSafeObservableCollection()
        {
            _lstData = new EnhancedObservableCollection<T>();
        }

        public ThreadSafeObservableCollection(IEnumerable<T> collection)
        {
            _lstData = new EnhancedObservableCollection<T>(collection);
        }

        public ThreadSafeObservableCollection(List<T> list)
        {
            _lstData = new EnhancedObservableCollection<T>(list);
        }

        /// <inheritdoc cref="ObservableCollection{T}.Count" />
        public int Count
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _lstData.Count;
            }
        }

        /// <inheritdoc />
        public object SyncRoot => LockObject;

        /// <inheritdoc />
        public bool IsSynchronized => true;
        
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public bool IsFixedSize => false;

        public ValueTask<int> CountAsync => GetCountAsync();

        public async ValueTask<int> GetCountAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token))
                return _lstData.Count;
        }

        public void Remove(object value)
        {
            Remove((T) value);
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            using (LockObject.EnterWriteLock())
                return _lstData.Remove(item);
        }

        /// <inheritdoc cref="List{T}.Remove(T)" />
        public async ValueTask<bool> RemoveAsync(T item)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                return _lstData.Remove(item);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.RemoveAt" />
        public void RemoveAt(int index)
        {
            using (LockObject.EnterWriteLock())
                _lstData.RemoveAt(index);
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.RemoveAt" />
        public async ValueTask RemoveAtAsync(int index)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                _lstData.RemoveAt(index);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
        }

        object IList.this[int index]
        {
            get => this[index];
            set => this[index] = (T)value;
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}" />
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

        public virtual int Add(object value)
        {
            if (!(value is T objCastValue))
                return -1;
            using (LockObject.EnterWriteLock())
            {
                _lstData.Add(objCastValue);
                return _lstData.Count - 1;
            }
        }

        /// <inheritdoc />
        public virtual void Add(T item)
        {
            using (LockObject.EnterWriteLock())
                _lstData.Add(item);
        }

        public virtual async ValueTask AddAsync(T item)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                _lstData.Add(item);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
        }

        /// <inheritdoc />
        public virtual bool TryAdd(T item)
        {
            Add(item);
            return true;
        }

        public void Clear()
        {
            using (LockObject.EnterWriteLock())
                _lstData.Clear();
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.Clear" />
        public async Task ClearAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                _lstData.Clear();
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
        }


        public void Insert(int index, object value)
        {
            Insert(index, (T) value);
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.Insert" />
        public virtual void Insert(int index, T item)
        {
            using (LockObject.EnterWriteLock())
                _lstData.Insert(index, item);
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.Insert" />
        public virtual async ValueTask InsertAsync(int index, T item)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                _lstData.Insert(index, item);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.Move" />
        public void Move(int oldIndex, int newIndex)
        {
            using (LockObject.EnterWriteLock())
                _lstData.Move(oldIndex, newIndex);
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.Move" />
        public async ValueTask MoveAsync(int oldIndex, int newIndex)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                _lstData.Move(oldIndex, newIndex);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
        }

        public bool Contains(object value)
        {
            return Contains((T) value);
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.Contains" />
        public bool Contains(T item)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.Contains(item);
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.Contains" />
        public async ValueTask<bool> ContainsAsync(T item)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.Contains(item);
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

        /// <inheritdoc cref="EnhancedObservableCollection{T}.CopyTo(T[],int)" />
        public void CopyTo(T[] array, int index)
        {
            using (EnterReadLock.Enter(LockObject))
                _lstData.CopyTo(array, index);
        }

        public async ValueTask CopyToAsync(T[] array, int index)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                _lstData.CopyTo(array, index);
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

        /// <inheritdoc />
        public T[] ToArray()
        {
            using (EnterReadLock.Enter(LockObject))
            {
                T[] aobjReturn = new T[Count];
                for (int i = 0; i < Count; ++i)
                {
                    aobjReturn[i] = _lstData[i];
                }
                return aobjReturn;
            }
        }

        public async ValueTask<T[]> ToArrayAsync()
        {
            using (await EnterReadLock.EnterAsync(LockObject))
            {
                T[] aobjReturn = new T[Count];
                for (int i = 0; i < Count; ++i)
                {
                    aobjReturn[i] = _lstData[i];
                }
                return aobjReturn;
            }
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            LockingEnumerator<T> objReturn = LockingEnumerator<T>.Get(this);
            objReturn.SetEnumerator(_lstData.GetEnumerator());
            return objReturn;
        }

        public async ValueTask<IEnumerator<T>> GetEnumeratorAsync(CancellationToken token = default)
        {
            LockingEnumerator<T> objReturn = await LockingEnumerator<T>.GetAsync(this, token);
            objReturn.SetEnumerator(_lstData.GetEnumerator());
            return objReturn;
        }

        public int IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.IndexOf(T)" />
        public int IndexOf(T item)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstData.IndexOf(item);
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.IndexOf(T)" />
        public async ValueTask<int> IndexOfAsync(T item)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _lstData.IndexOf(item);
        }


        /// <inheritdoc cref="List{T}.Sort()" />
        public void Sort()
        {
            using (LockObject.EnterWriteLock())
            {
                T[] aobjSorted = new T[_lstData.Count];
                for (int i = 0; i < _lstData.Count; ++i)
                    aobjSorted[i] = _lstData[i];
                Array.Sort(aobjSorted);
                for (int i = 0; i < aobjSorted.Length; ++i)
                    _lstData.Move(_lstData.IndexOf(aobjSorted[i]), i);
            }
        }

        /// <inheritdoc cref="List{T}.Sort(IComparer{T})" />
        public void Sort(IComparer<T> comparer)
        {
            using (LockObject.EnterWriteLock())
            {
                T[] aobjSorted = new T[_lstData.Count];
                for (int i = 0; i < _lstData.Count; ++i)
                    aobjSorted[i] = _lstData[i];
                Array.Sort(aobjSorted, comparer);
                for (int i = 0; i < aobjSorted.Length; ++i)
                    _lstData.Move(_lstData.IndexOf(aobjSorted[i]), i);
            }
        }

        /// <inheritdoc cref="List{T}.Sort(int, int, IComparer{T})" />
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            using (LockObject.EnterWriteLock())
            {
                T[] aobjSorted = new T[count];
                for (int i = 0; i < count; ++i)
                    aobjSorted[i] = _lstData[index + i];
                Array.Sort(aobjSorted, comparer);
                for (int i = 0; i < aobjSorted.Length; ++i)
                    _lstData.Move(_lstData.IndexOf(aobjSorted[i]), index + i);
            }
        }

        /// <inheritdoc cref="List{T}.Sort(Comparison{T})" />
        public void Sort(Comparison<T> comparison)
        {
            using (LockObject.EnterWriteLock())
            {
                T[] aobjSorted = new T[_lstData.Count];
                for (int i = 0; i < _lstData.Count; ++i)
                    aobjSorted[i] = _lstData[i];
                Array.Sort(aobjSorted, comparison);
                for (int i = 0; i < aobjSorted.Length; ++i)
                    _lstData.Move(_lstData.IndexOf(aobjSorted[i]), i);
            }
        }

        /// <inheritdoc cref="List{T}.Sort()" />
        public async ValueTask SortAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                T[] aobjSorted = new T[_lstData.Count];
                for (int i = 0; i < _lstData.Count; ++i)
                    aobjSorted[i] = _lstData[i];
                Array.Sort(aobjSorted);
                for (int i = 0; i < aobjSorted.Length; ++i)
                    _lstData.Move(_lstData.IndexOf(aobjSorted[i]), i);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
        }

        /// <inheritdoc cref="List{T}.Sort(IComparer{T})" />
        public async ValueTask SortAsync(IComparer<T> comparer)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                T[] aobjSorted = new T[_lstData.Count];
                for (int i = 0; i < _lstData.Count; ++i)
                    aobjSorted[i] = _lstData[i];
                Array.Sort(aobjSorted, comparer);
                for (int i = 0; i < aobjSorted.Length; ++i)
                    _lstData.Move(_lstData.IndexOf(aobjSorted[i]), i);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
        }

        /// <inheritdoc cref="List{T}.Sort(int, int, IComparer{T})" />
        public async ValueTask SortAsync(int index, int count, IComparer<T> comparer)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                T[] aobjSorted = new T[count];
                for (int i = 0; i < count; ++i)
                    aobjSorted[i] = _lstData[index + i];
                Array.Sort(aobjSorted, comparer);
                for (int i = 0; i < aobjSorted.Length; ++i)
                    _lstData.Move(_lstData.IndexOf(aobjSorted[i]), index + i);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
        }

        /// <inheritdoc cref="List{T}.Sort(Comparison{T})" />
        public async ValueTask SortAsync(Comparison<T> comparison)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                T[] aobjSorted = new T[_lstData.Count];
                for (int i = 0; i < _lstData.Count; ++i)
                    aobjSorted[i] = _lstData[i];
                Array.Sort(aobjSorted, comparison);
                for (int i = 0; i < aobjSorted.Length; ++i)
                    _lstData.Move(_lstData.IndexOf(aobjSorted[i]), i);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
        }

        /// <inheritdoc />
        public virtual event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                using (LockObject.EnterWriteLock())
                    _lstData.CollectionChanged += value;
            }
            remove
            {
                using (LockObject.EnterWriteLock())
                    _lstData.CollectionChanged -= value;
            }
        }

        /// <inheritdoc cref="EnhancedObservableCollection{T}.BeforeClearCollectionChanged" />
        public virtual event NotifyCollectionChangedEventHandler BeforeClearCollectionChanged
        {
            add
            {
                using (LockObject.EnterWriteLock())
                    _lstData.BeforeClearCollectionChanged += value;
            }
            remove
            {
                using (LockObject.EnterWriteLock())
                    _lstData.BeforeClearCollectionChanged -= value;
            }
        }

        /// <inheritdoc />
        public virtual event PropertyChangedEventHandler PropertyChanged;


        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            Utils.RunOnMainThread(() =>
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyName));
                }
            });
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
