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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

namespace Chummer
{
    public class ThreadSafeObservableCollection<T> : EnhancedObservableCollection<T>, IDisposable, IProducerConsumerCollection<T>
    {
        protected ReaderWriterLockSlim LockerObject { get; } = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public ThreadSafeObservableCollection()
        {
        }

        public ThreadSafeObservableCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        public ThreadSafeObservableCollection(List<T> list) : base(list)
        {
        }

        /// <inheritdoc />
        public new int Count
        {
            get
            {
                using (new EnterReadLock(LockerObject))
                    return base.Count;
            }
        }

        /// <inheritdoc cref="List{T}" />
        public new T this[int index]
        {
            get
            {
                using (new EnterReadLock(LockerObject))
                    return base[index];
            }
            set
            {
                using (new EnterUpgradeableReadLock(LockerObject))
                {
                    if (base[index].Equals(value))
                        return;
                    using (new EnterWriteLock(LockerObject))
                        base[index] = value;
                }
            }
        }

        /// <inheritdoc cref="List{T}.Contains" />
        public new bool Contains(T item)
        {
            using (new EnterReadLock(LockerObject))
                return base.Contains(item);
        }

        /// <inheritdoc />
        public new void CopyTo(T[] array, int index)
        {
            using (new EnterReadLock(LockerObject))
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
            using (new EnterWriteLock(LockerObject))
            {
                if (Count > 0)
                {
                    // FIFO to be compliant with how the default for BlockingCollection<T> is ConcurrentQueue
                    item = this[0];
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
            using (new EnterReadLock(LockerObject))
            {
                T[] aobjReturn = new T[Count];
                for (int i = 0; i < Count; ++i)
                {
                    aobjReturn[i] = this[i];
                }
                return aobjReturn;
            }
        }

        /// <inheritdoc />
        public new IEnumerator<T> GetEnumerator()
        {
            using (new EnterReadLock(LockerObject))
                return base.GetEnumerator().GetLockingType(LockerObject);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T)" />
        public new int IndexOf(T item)
        {
            using (new EnterReadLock(LockerObject))
                return base.IndexOf(item);
        }

        /// <inheritdoc />
        public override event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                using (new EnterWriteLock(LockerObject))
                    base.CollectionChanged += value;
            }
            remove
            {
                using (new EnterWriteLock(LockerObject))
                    base.CollectionChanged -= value;
            }
        }

        /// <inheritdoc />
        protected override event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                using (new EnterWriteLock(LockerObject))
                    base.PropertyChanged += value;
            }
            remove
            {
                using (new EnterWriteLock(LockerObject))
                    base.PropertyChanged -= value;
            }
        }

        /// <inheritdoc />
        protected override void InsertItem(int index, T item)
        {
            using (new EnterWriteLock(LockerObject))
                base.InsertItem(index, item);
        }

        /// <inheritdoc />
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            using (new EnterWriteLock(LockerObject))
                base.MoveItem(oldIndex, newIndex);
        }

        /// <inheritdoc />
        protected override void ClearItems()
        {
            using (new EnterWriteLock(LockerObject))
                base.ClearItems();
        }

        /// <inheritdoc />
        protected override void RemoveItem(int index)
        {
            using (new EnterWriteLock(LockerObject))
                base.RemoveItem(index);
        }

        /// <inheritdoc />
        protected override void SetItem(int index, T item)
        {
            using (new EnterWriteLock(LockerObject))
                base.SetItem(index, item);
        }

        /// <inheritdoc />
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            using (new EnterUpgradeableReadLock(LockerObject))
                base.OnPropertyChanged(e);
        }

        /// <inheritdoc />
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            using (new EnterUpgradeableReadLock(LockerObject))
                base.OnCollectionChanged(e);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                LockerObject.Dispose();
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
