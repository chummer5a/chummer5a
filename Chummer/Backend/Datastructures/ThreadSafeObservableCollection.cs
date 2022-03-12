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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Chummer
{
    public class ThreadSafeObservableCollection<T> : EnhancedObservableCollection<T>, IProducerConsumerCollection<T>, IHasLockObject, IAsyncReadOnlyCollection<T>
    {
        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();

        public ThreadSafeObservableCollection()
        {
        }

        public ThreadSafeObservableCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        public ThreadSafeObservableCollection(List<T> list) : base(list)
        {
        }

        /// <inheritdoc cref="ObservableCollection{T}.Count" />
        public new int Count
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return base.Count;
            }
        }

        public async ValueTask<int> GetCountAsync()
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.Count;
        }

        /// <inheritdoc cref="List{T}" />
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
                    using (LockObject.EnterWriteLock())
                        base[index] = value;
                }
            }
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

        /// <inheritdoc />
        public new void CopyTo(T[] array, int index)
        {
            using (EnterReadLock.Enter(LockObject))
                base.CopyTo(array, index);
        }

        public async ValueTask CopyToAsync(T[] array, int index)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                base.CopyTo(array, index);
        }

        /// <inheritdoc />
        public virtual bool TryAdd(T item)
        {
            Add(item);
            return true;
        }

        /// <inheritdoc />
        public bool TryTake(out T item)
        {
            // Immediately enter a write lock to prevent attempted reads until we have either taken the item we want to take or failed to do so
            using (LockObject.EnterWriteLock())
            {
                if (base.Count > 0)
                {
                    // FIFO to be compliant with how the default for BlockingCollection<T> is ConcurrentQueue
                    item = base[0];
                    base.RemoveItem(0);
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
                    aobjReturn[i] = base[i];
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
                    aobjReturn[i] = base[i];
                }
                return aobjReturn;
            }
        }

        /// <inheritdoc />
        public new IEnumerator<T> GetEnumerator()
        {
            LockingEnumerator<T> objReturn = LockingEnumerator<T>.Get(this);
            objReturn.SetEnumerator(base.GetEnumerator());
            return objReturn;
        }

        public async ValueTask<IEnumerator<T>> GetEnumeratorAsync()
        {
            LockingEnumerator<T> objReturn = await LockingEnumerator<T>.GetAsync(this);
            objReturn.SetEnumerator(base.GetEnumerator());
            return objReturn;
        }

        /// <inheritdoc cref="List{T}.IndexOf(T)" />
        public new int IndexOf(T item)
        {
            using (EnterReadLock.Enter(LockObject))
                return base.IndexOf(item);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T)" />
        public async ValueTask<int> IndexOfAsync(T item)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return base.IndexOf(item);
        }

        /// <inheritdoc />
        public override event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                using (LockObject.EnterWriteLock())
                    base.CollectionChanged += value;
            }
            remove
            {
                using (LockObject.EnterWriteLock())
                    base.CollectionChanged -= value;
            }
        }

        /// <inheritdoc />
        public override event NotifyCollectionChangedEventHandler BeforeClearCollectionChanged
        {
            add
            {
                using (LockObject.EnterWriteLock())
                    base.BeforeClearCollectionChanged += value;
            }
            remove
            {
                using (LockObject.EnterWriteLock())
                    base.BeforeClearCollectionChanged -= value;
            }
        }

        /// <inheritdoc />
        protected override event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                using (LockObject.EnterWriteLock())
                    base.PropertyChanged += value;
            }
            remove
            {
                using (LockObject.EnterWriteLock())
                    base.PropertyChanged -= value;
            }
        }

        /// <inheritdoc />
        protected override void InsertItem(int index, T item)
        {
            using (LockObject.EnterWriteLock())
                base.InsertItem(index, item);
        }

        /// <inheritdoc />
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            using (LockObject.EnterWriteLock())
                base.MoveItem(oldIndex, newIndex);
        }

        /// <inheritdoc />
        protected override void ClearItems()
        {
            using (LockObject.EnterWriteLock())
                base.ClearItems();
        }

        /// <inheritdoc />
        protected override void RemoveItem(int index)
        {
            using (LockObject.EnterWriteLock())
                base.RemoveItem(index);
        }

        /// <inheritdoc />
        protected override void SetItem(int index, T item)
        {
            using (LockObject.EnterWriteLock())
                base.SetItem(index, item);
        }

        /// <inheritdoc />
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            using (EnterReadLock.Enter(LockObject))
                base.OnPropertyChanged(e);
        }

        /// <inheritdoc />
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            using (EnterReadLock.Enter(LockObject))
                base.OnCollectionChanged(e);
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
    }
}
