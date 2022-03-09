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
using System.ComponentModel;
using System.Threading.Tasks;

namespace Chummer
{
    public class ThreadSafeBindingList<T> : CachedBindingList<T>, IHasLockObject, IProducerConsumerCollection<T>, IAsyncEnumerable<T>
    {
        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();

        /// <inheritdoc />
        public override event EventHandler<RemovingOldEventArgs> BeforeRemove
        {
            add
            {
                using (LockObject.EnterWriteLock())
                    base.BeforeRemove += value;
            }
            remove
            {
                using (LockObject.EnterWriteLock())
                    base.BeforeRemove -= value;
            }
        }

        /// <inheritdoc cref="BindingList{T}.AddingNew" />
        public new event AddingNewEventHandler AddingNew
        {
            add
            {
                using (LockObject.EnterWriteLock())
                    base.AddingNew += value;
            }
            remove
            {
                using (LockObject.EnterWriteLock())
                    base.AddingNew -= value;
            }
        }

        /// <inheritdoc cref="BindingList{T}.ListChanged" />
        public new event ListChangedEventHandler ListChanged
        {
            add
            {
                using (LockObject.EnterWriteLock())
                    base.ListChanged += value;
            }
            remove
            {
                using (LockObject.EnterWriteLock())
                    base.ListChanged -= value;
            }
        }

        /// <inheritdoc />
        protected override void OnAddingNew(AddingNewEventArgs e)
        {
            using (EnterReadLock.Enter(LockObject))
                base.OnAddingNew(e);
        }

        /// <inheritdoc />
        protected override void OnListChanged(ListChangedEventArgs e)
        {
            using (EnterReadLock.Enter(LockObject))
                base.OnListChanged(e);
        }

        /// <inheritdoc cref="BindingList{T}.Count" />
        public new int Count
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return base.Count;
            }
        }
        
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

        /// <inheritdoc cref="BindingList{T}.RaiseListChangedEvents" />
        public new bool RaiseListChangedEvents
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return base.RaiseListChangedEvents;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (base.RaiseListChangedEvents.Equals(value))
                        return;
                    using (LockObject.EnterWriteLock())
                        base.RaiseListChangedEvents = value;
                }
            }
        }

        /// <inheritdoc cref="BindingList{T}.AllowNew" />
        public new bool AllowNew
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return base.AllowNew;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (base.AllowNew.Equals(value))
                        return;
                    using (LockObject.EnterWriteLock())
                        base.AllowNew = value;
                }
            }
        }

        /// <inheritdoc cref="BindingList{T}.AllowEdit" />
        public new bool AllowEdit
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return base.AllowEdit;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (base.AllowEdit.Equals(value))
                        return;
                    using (LockObject.EnterWriteLock())
                        base.AllowEdit = value;
                }
            }
        }

        /// <inheritdoc cref="BindingList{T}.AllowRemove" />
        public new bool AllowRemove
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return base.AllowRemove;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (base.AllowRemove.Equals(value))
                        return;
                    using (LockObject.EnterWriteLock())
                        base.AllowRemove = value;
                }
            }
        }

        /// <inheritdoc cref="BindingList{T}.GetEnumerator" />
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

        /// <inheritdoc cref="BindingList{T}.IndexOf" />
        public new int IndexOf(T item)
        {
            using (EnterReadLock.Enter(LockObject))
                return base.IndexOf(item);
        }

        /// <inheritdoc />
        protected override void InsertItem(int index, T item)
        {
            using (LockObject.EnterWriteLock())
                base.InsertItem(index, item);
        }

        /// <inheritdoc />
        protected override void RemoveItem(int index)
        {
            using (LockObject.EnterWriteLock())
                base.RemoveItem(index);
        }

        /// <inheritdoc />
        protected override void ClearItems()
        {
            using (LockObject.EnterWriteLock())
                base.ClearItems();
        }

        /// <inheritdoc />
        protected override void SetItem(int index, T item)
        {
            using (LockObject.EnterWriteLock())
                base.SetItem(index, item);
        }

        /// <inheritdoc />
        public override void CancelNew(int itemIndex)
        {
            using (LockObject.EnterWriteLock())
                base.CancelNew(itemIndex);
        }

        /// <inheritdoc />
        public override void EndNew(int itemIndex)
        {
            using (LockObject.EnterWriteLock())
                base.EndNew(itemIndex);
        }

        /// <inheritdoc />
        protected override object AddNewCore()
        {
            using (LockObject.EnterWriteLock())
                return base.AddNewCore();
        }

        /// <inheritdoc />
        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            using (LockObject.EnterWriteLock())
                base.ApplySortCore(prop, direction);
        }

        /// <inheritdoc />
        protected override void RemoveSortCore()
        {
            using (LockObject.EnterWriteLock())
                base.RemoveSortCore();
        }

        /// <inheritdoc />
        protected override int FindCore(PropertyDescriptor prop, object key)
        {
            using (LockObject.EnterWriteLock())
                return base.FindCore(prop, key);
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

        /// <inheritdoc />
        public bool TryAdd(T item)
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
                    RemoveAt(0);
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
                T[] aobjReturn = new T[base.Count];
                for (int i = 0; i < aobjReturn.Length; ++i)
                    aobjReturn[i] = base[i];
                return aobjReturn;
            }
        }
    }
}
