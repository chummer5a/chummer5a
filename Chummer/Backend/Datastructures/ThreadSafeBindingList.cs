using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;

namespace Chummer
{
    public class ThreadSafeBindingList<T> : CachedBindingList<T>, IHasLockObject, IProducerConsumerCollection<T>
    {
        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();

        /// <inheritdoc />
        public override event EventHandler<RemovingOldEventArgs> BeforeRemove
        {
            add
            {
                using (new EnterWriteLock(LockObject))
                    base.BeforeRemove += value;
            }
            remove
            {
                using (new EnterWriteLock(LockObject))
                    base.BeforeRemove -= value;
            }
        }

        /// <inheritdoc cref="BindingList{T}.AddingNew" />
        public new event AddingNewEventHandler AddingNew
        {
            add
            {
                using (new EnterWriteLock(LockObject))
                    base.AddingNew += value;
            }
            remove
            {
                using (new EnterWriteLock(LockObject))
                    base.AddingNew -= value;
            }
        }

        /// <inheritdoc cref="BindingList{T}.ListChanged" />
        public new event ListChangedEventHandler ListChanged
        {
            add
            {
                using (new EnterWriteLock(LockObject))
                    base.ListChanged += value;
            }
            remove
            {
                using (new EnterWriteLock(LockObject))
                    base.ListChanged -= value;
            }
        }

        /// <inheritdoc />
        protected override void OnAddingNew(AddingNewEventArgs e)
        {
            using (new EnterReadLock(LockObject))
                base.OnAddingNew(e);
        }

        /// <inheritdoc />
        protected override void OnListChanged(ListChangedEventArgs e)
        {
            using (new EnterReadLock(LockObject))
                base.OnListChanged(e);
        }

        /// <inheritdoc cref="BindingList{T}.Count" />
        public new int Count
        {
            get
            {
                using (new EnterReadLock(LockObject))
                    return base.Count;
            }
        }
        
        public new T this[int index]
        {
            get
            {
                using (new EnterReadLock(LockObject))
                    return base[index];
            }
            set
            {
                using (new EnterReadLock(LockObject))
                {
                    if (base[index].Equals(value))
                        return;
                    using (new EnterWriteLock(LockObject))
                        base[index] = value;
                }
            }
        }

        /// <inheritdoc cref="BindingList{T}.RaiseListChangedEvents" />
        public new bool RaiseListChangedEvents
        {
            get
            {
                using (new EnterReadLock(LockObject))
                    return base.RaiseListChangedEvents;
            }
            set
            {
                using (new EnterReadLock(LockObject))
                {
                    if (base.RaiseListChangedEvents.Equals(value))
                        return;
                    using (new EnterWriteLock(LockObject))
                        base.RaiseListChangedEvents = value;
                }
            }
        }

        /// <inheritdoc cref="BindingList{T}.AllowNew" />
        public new bool AllowNew
        {
            get
            {
                using (new EnterReadLock(LockObject))
                    return base.AllowNew;
            }
            set
            {
                using (new EnterReadLock(LockObject))
                {
                    if (base.AllowNew.Equals(value))
                        return;
                    using (new EnterWriteLock(LockObject))
                        base.AllowNew = value;
                }
            }
        }

        /// <inheritdoc cref="BindingList{T}.AllowEdit" />
        public new bool AllowEdit
        {
            get
            {
                using (new EnterReadLock(LockObject))
                    return base.AllowEdit;
            }
            set
            {
                using (new EnterReadLock(LockObject))
                {
                    if (base.AllowEdit.Equals(value))
                        return;
                    using (new EnterWriteLock(LockObject))
                        base.AllowEdit = value;
                }
            }
        }

        /// <inheritdoc cref="BindingList{T}.AllowRemove" />
        public new bool AllowRemove
        {
            get
            {
                using (new EnterReadLock(LockObject))
                    return base.AllowRemove;
            }
            set
            {
                using (new EnterReadLock(LockObject))
                {
                    if (base.AllowRemove.Equals(value))
                        return;
                    using (new EnterWriteLock(LockObject))
                        base.AllowRemove = value;
                }
            }
        }

        /// <inheritdoc cref="BindingList{T}.GetEnumerator" />
        public new IEnumerator<T> GetEnumerator()
        {
            LockingEnumerator<T> objReturn = new LockingEnumerator<T>(this);
            objReturn.SetEnumerator(base.GetEnumerator());
            return objReturn;
        }

        /// <inheritdoc cref="BindingList{T}.IndexOf" />
        public new int IndexOf(T item)
        {
            using (new EnterWriteLock(LockObject))
                return base.IndexOf(item);
        }

        /// <inheritdoc cref="BindingList{T}.ResetBindings" />
        public new void ResetBindings()
        {
            using (new EnterWriteLock(LockObject))
                base.ResetBindings();
        }

        /// <inheritdoc cref="BindingList{T}.ResetItem" />
        public new void ResetItem(int position)
        {
            using (new EnterWriteLock(LockObject))
                base.ResetItem(position);
        }

        /// <inheritdoc cref="BindingList{T}.AddNew" />
        public new T AddNew()
        {
            using (new EnterWriteLock(LockObject))
                return base.AddNew();
        }

        /// <inheritdoc />
        protected override void InsertItem(int index, T item)
        {
            using (new EnterWriteLock(LockObject))
                base.InsertItem(index, item);
        }

        /// <inheritdoc />
        protected override void RemoveItem(int index)
        {
            using (new EnterWriteLock(LockObject))
                base.RemoveItem(index);
        }

        /// <inheritdoc />
        protected override void ClearItems()
        {
            using (new EnterWriteLock(LockObject))
                base.ClearItems();
        }

        /// <inheritdoc />
        protected override void SetItem(int index, T item)
        {
            using (new EnterWriteLock(LockObject))
                base.SetItem(index, item);
        }

        /// <inheritdoc />
        public override void CancelNew(int itemIndex)
        {
            using (new EnterWriteLock(LockObject))
                base.CancelNew(itemIndex);
        }

        /// <inheritdoc />
        public override void EndNew(int itemIndex)
        {
            using (new EnterWriteLock(LockObject))
                base.EndNew(itemIndex);
        }

        /// <inheritdoc />
        protected override object AddNewCore()
        {
            using (new EnterWriteLock(LockObject))
                return base.AddNewCore();
        }

        /// <inheritdoc />
        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            using (new EnterWriteLock(LockObject))
                base.ApplySortCore(prop, direction);
        }

        /// <inheritdoc />
        protected override void RemoveSortCore()
        {
            using (new EnterWriteLock(LockObject))
                base.RemoveSortCore();
        }

        /// <inheritdoc />
        protected override int FindCore(PropertyDescriptor prop, object key)
        {
            using (new EnterWriteLock(LockObject))
                return base.FindCore(prop, key);
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
            using (new EnterWriteLock(LockObject))
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
            using (new EnterReadLock(LockObject))
            {
                T[] aobjReturn = new T[base.Count];
                for (int i = 0; i < aobjReturn.Length; ++i)
                    aobjReturn[i] = base[i];
                return aobjReturn;
            }
        }
    }
}
