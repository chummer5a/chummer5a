using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

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
                using (EnterWriteLock.Enter(LockObject))
                    base.BeforeRemove += value;
            }
            remove
            {
                using (EnterWriteLock.Enter(LockObject))
                    base.BeforeRemove -= value;
            }
        }

        /// <inheritdoc cref="BindingList{T}.AddingNew" />
        public new event AddingNewEventHandler AddingNew
        {
            add
            {
                using (EnterWriteLock.Enter(LockObject))
                    base.AddingNew += value;
            }
            remove
            {
                using (EnterWriteLock.Enter(LockObject))
                    base.AddingNew -= value;
            }
        }

        /// <inheritdoc cref="BindingList{T}.ListChanged" />
        public new event ListChangedEventHandler ListChanged
        {
            add
            {
                using (EnterWriteLock.Enter(LockObject))
                    base.ListChanged += value;
            }
            remove
            {
                using (EnterWriteLock.Enter(LockObject))
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
                    using (EnterWriteLock.Enter(LockObject))
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
                    using (EnterWriteLock.Enter(LockObject))
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
                    using (EnterWriteLock.Enter(LockObject))
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
                    using (EnterWriteLock.Enter(LockObject))
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
                    using (EnterWriteLock.Enter(LockObject))
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
            using (EnterReadLock.Enter(LockObject))
                return base.IndexOf(item);
        }

        /// <inheritdoc />
        protected override void InsertItem(int index, T item)
        {
            using (EnterWriteLock.Enter(LockObject))
                base.InsertItem(index, item);
        }

        /// <inheritdoc />
        protected override void RemoveItem(int index)
        {
            using (EnterWriteLock.Enter(LockObject))
                base.RemoveItem(index);
        }

        /// <inheritdoc />
        protected override void ClearItems()
        {
            using (EnterWriteLock.Enter(LockObject))
                base.ClearItems();
        }

        /// <inheritdoc />
        protected override void SetItem(int index, T item)
        {
            using (EnterWriteLock.Enter(LockObject))
                base.SetItem(index, item);
        }

        /// <inheritdoc />
        public override void CancelNew(int itemIndex)
        {
            using (EnterWriteLock.Enter(LockObject))
                base.CancelNew(itemIndex);
        }

        /// <inheritdoc />
        public override void EndNew(int itemIndex)
        {
            using (EnterWriteLock.Enter(LockObject))
                base.EndNew(itemIndex);
        }

        /// <inheritdoc />
        protected override object AddNewCore()
        {
            using (EnterWriteLock.Enter(LockObject))
                return base.AddNewCore();
        }

        /// <inheritdoc />
        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            using (EnterWriteLock.Enter(LockObject))
                base.ApplySortCore(prop, direction);
        }

        /// <inheritdoc />
        protected override void RemoveSortCore()
        {
            using (EnterWriteLock.Enter(LockObject))
                base.RemoveSortCore();
        }

        /// <inheritdoc />
        protected override int FindCore(PropertyDescriptor prop, object key)
        {
            using (EnterWriteLock.Enter(LockObject))
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
            using (EnterWriteLock.Enter(LockObject))
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
