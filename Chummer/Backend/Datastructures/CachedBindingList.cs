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
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    public class CachedBindingList<T> : BindingList<T>, IAsyncList<T>, IAsyncReadOnlyList<T>, IDisposable, IAsyncDisposable
    {
        public AsyncFriendlyReaderWriterLock BindingListLock { get; set; }

        [NonSerialized]
        private PropertyDescriptorCollection itemTypeProperties;
        [NonSerialized]
        private PropertyChangedAsyncEventHandler propertyChangedAsyncEventHandler;
        [NonSerialized]
        private int _intLastAsyncChangeIndex = -1;

#pragma warning disable CA1070
        /// <summary>
        /// ListChanged event subscription that will fire right before an item is removed or the list is cleared.
        /// </summary>
        public virtual event EventHandler<RemovingOldEventArgs> BeforeRemove;

        private readonly List<AsyncBeforeRemoveEventHandler> _lstBeforeRemoveAsync =
            new List<AsyncBeforeRemoveEventHandler>();
        private readonly List<AsyncAddingNewEventHandler> _lstAddingNewAsync =
            new List<AsyncAddingNewEventHandler>();
        private readonly List<AsyncListChangedEventHandler> _lstListChangedAsync =
            new List<AsyncListChangedEventHandler>();

        /// <summary>
        /// ListChanged event subscription that will fire right before an item is removed or the list is cleared.
        /// Use this event instead of BeforeRemove for tasks that will be awaited before completion.
        /// </summary>
        public virtual event AsyncBeforeRemoveEventHandler BeforeRemoveAsync
        {
            add => _lstBeforeRemoveAsync.Add(value);
            remove => _lstBeforeRemoveAsync.Remove(value);
        }

        /// <summary>
        /// Like AddingNew, occurs when an item is added.
        /// Use this event instead of AddingNew for tasks that will be awaited before completion.
        /// </summary>
        public virtual event AsyncAddingNewEventHandler AddingNewAsync
        {
            add => _lstAddingNewAsync.Add(value);
            remove => _lstAddingNewAsync.Remove(value);
        }

        /// <summary>
        /// Like ListChanged, occurs when an item is added, removed, changed, moved, or the entire list is refreshed.
        /// Use this event instead of ListChanged for tasks that will be awaited before completion.
        /// </summary>
        public virtual event AsyncListChangedEventHandler ListChangedAsync
        {
            add => _lstListChangedAsync.Add(value);
            remove => _lstListChangedAsync.Remove(value);
        }
#pragma warning restore CA1070

        public CachedBindingList()
        {
        }

        public CachedBindingList(IList<T> list) : base(list)
        {
            foreach (T obj in Items)
                HookAsyncPropertyChanged(obj);
        }

        /// <inheritdoc />
        protected override void RemoveItem(int index)
        {
            if (RaiseListChangedEvents)
            {
                IDisposable objLocker = BindingListLock?.EnterReadLock();
                try
                {
                    if (_lstBeforeRemoveAsync.Count > 0)
                    {
                        RemovingOldEventArgs objArgs = new RemovingOldEventArgs(Items[index], index);
                        Func<Task>[] aFuncs = new Func<Task>[_lstBeforeRemoveAsync.Count];
                        int i = 0;
                        foreach (AsyncBeforeRemoveEventHandler objEvent in _lstBeforeRemoveAsync)
                        {
                            aFuncs[i++] = () => objEvent.Invoke(this, objArgs);
                        }

                        Utils.RunWithoutThreadLock(aFuncs);
                        if (BeforeRemove != null)
                        {
                            Utils.RunOnMainThread(() => BeforeRemove?.Invoke(this, objArgs));
                        }
                    }
                    else if (BeforeRemove != null)
                    {
                        Utils.RunOnMainThread(() => BeforeRemove?.Invoke(this, new RemovingOldEventArgs(Items[index], index)));
                    }
                }
                finally
                {
                    objLocker?.Dispose();
                }
            }

            UnhookAsyncPropertyChanged(this[index]);

            base.RemoveItem(index);
        }

        /// <inheritdoc />
        protected override void ClearItems()
        {
            if (RaiseListChangedEvents)
            {
                IDisposable objLocker = BindingListLock?.EnterReadLock();
                try
                {
                    if (_lstBeforeRemoveAsync.Count > 0)
                    {
                        List<RemovingOldEventArgs> lstArgsList = new List<RemovingOldEventArgs>();
                        for (int i = 0; i < Items.Count; ++i)
                            lstArgsList.Add(new RemovingOldEventArgs(Items[i], i));
                        Func<Task>[] aFuncs = new Func<Task>[lstArgsList.Count * _lstBeforeRemoveAsync.Count];
                        int j = 0;
                        foreach (AsyncBeforeRemoveEventHandler objEvent in _lstBeforeRemoveAsync)
                        {
                            foreach (RemovingOldEventArgs objArgs in lstArgsList)
                                aFuncs[j++] = () => objEvent.Invoke(this, objArgs);
                        }

                        Utils.RunWithoutThreadLock(aFuncs);
                        if (BeforeRemove != null)
                        {
                            Utils.RunOnMainThread(() =>
                            {
                                if (BeforeRemove != null)
                                {
                                    foreach (RemovingOldEventArgs objArgs in lstArgsList)
                                    {
                                        BeforeRemove.Invoke(this, objArgs);
                                    }
                                }
                            });
                        }
                    }
                    else if (BeforeRemove != null)
                    {
                        Utils.RunOnMainThread(() =>
                        {
                            if (BeforeRemove != null)
                            {
                                for (int i = 0; i < Items.Count; ++i)
                                {
                                    BeforeRemove.Invoke(this, new RemovingOldEventArgs(Items[i], i));
                                }
                            }
                        });
                    }
                }
                finally
                {
                    objLocker?.Dispose();
                }
            }

            foreach (T obj in Items)
                UnhookAsyncPropertyChanged(obj);

            base.ClearItems();
        }

        /// <inheritdoc />
        protected override void SetItem(int index, T item)
        {
            if (RaiseListChangedEvents)
            {
                IDisposable objLocker = BindingListLock?.EnterReadLock();
                try
                {
                    T objOldItem = Items[index];
                    if (!ReferenceEquals(objOldItem, item))
                    {
                        if (_lstBeforeRemoveAsync.Count > 0)
                        {
                            RemovingOldEventArgs objArgs = new RemovingOldEventArgs(Items[index], index);
                            Func<Task>[] aFuncs = new Func<Task>[_lstBeforeRemoveAsync.Count];
                            int i = 0;
                            foreach (AsyncBeforeRemoveEventHandler objEvent in _lstBeforeRemoveAsync)
                            {
                                aFuncs[i++] = () => objEvent.Invoke(this, objArgs);
                            }

                            Utils.RunWithoutThreadLock(aFuncs);
                            if (BeforeRemove != null)
                            {
                                Utils.RunOnMainThread(() => BeforeRemove?.Invoke(this, objArgs));
                            }
                        }
                        else if (BeforeRemove != null)
                        {
                            Utils.RunOnMainThread(() =>
                                BeforeRemove?.Invoke(this, new RemovingOldEventArgs(Items[index], index)));
                        }
                    }
                }
                finally
                {
                    objLocker?.Dispose();
                }
            }

            UnhookAsyncPropertyChanged(this[index]);
            HookAsyncPropertyChanged(item);

            base.SetItem(index, item);
        }

        /// <inheritdoc />
        protected override void OnAddingNew(AddingNewEventArgs e)
        {
            IDisposable objLocker = BindingListLock?.EnterReadLock();
            try
            {
                if (_lstAddingNewAsync.Count > 0)
                {
                    Func<Task>[] aFuncs = new Func<Task>[_lstAddingNewAsync.Count];
                    int i = 0;
                    foreach (AsyncAddingNewEventHandler objEvent in _lstAddingNewAsync)
                        aFuncs[i++] = () => objEvent.Invoke(this, e);

                    Utils.RunWithoutThreadLock(aFuncs);
                }

                base.OnAddingNew(e);
            }
            finally
            {
                objLocker?.Dispose();
            }
        }

        /// <inheritdoc />
        protected override void OnListChanged(ListChangedEventArgs e)
        {
            IDisposable objLocker = BindingListLock?.EnterReadLock();
            try
            {
                if (_lstListChangedAsync.Count > 0)
                {
                    Func<Task>[] aFuncs = new Func<Task>[_lstListChangedAsync.Count];
                    int i = 0;
                    foreach (AsyncListChangedEventHandler objEvent in _lstListChangedAsync)
                        aFuncs[i++] = () => objEvent.Invoke(this, e);

                    Utils.RunWithoutThreadLock(aFuncs);
                }

                base.OnListChanged(e);
            }
            finally
            {
                objLocker?.Dispose();
            }
        }

        protected virtual async Task OnListChangedAsync(ListChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IDisposable objLocker = BindingListLock != null ? await BindingListLock.EnterReadLockAsync(token).ConfigureAwait(false) : null;
            try
            {
                token.ThrowIfCancellationRequested();
                if (_lstListChangedAsync.Count > 0)
                {
                    List<Task> lstTasks = new List<Task>(Utils.MaxParallelBatchSize);
                    int i = 0;
                    foreach (AsyncListChangedEventHandler objEvent in _lstListChangedAsync)
                    {
                        lstTasks.Add(objEvent.Invoke(this, e, token));
                        if (++i < Utils.MaxParallelBatchSize)
                            continue;
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        lstTasks.Clear();
                        i = 0;
                    }

                    await Task.WhenAll(lstTasks).ConfigureAwait(false);
                }

                base.OnListChanged(e);
            }
            finally
            {
                objLocker?.Dispose();
            }
        }

        public Task<IEnumerator<T>> GetEnumeratorAsync(CancellationToken token = default)
        {
            return token.IsCancellationRequested
                ? Task.FromCanceled<IEnumerator<T>>(token)
                : Task.FromResult(GetEnumerator());
        }

        public Task<int> GetCountAsync(CancellationToken token)
        {
            return token.IsCancellationRequested
                ? Task.FromCanceled<int>(token)
                : Task.FromResult(Count);
        }

        public async Task AddAsync(T item, CancellationToken token = default)
        {
            await InsertAsync(await GetCountAsync(token).ConfigureAwait(false), item, token).ConfigureAwait(false);
        }

        public async Task ClearAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!RaiseListChangedEvents)
            {
                foreach (T obj in Items)
                    await UnhookAsyncPropertyChangedAsync(obj, token).ConfigureAwait(false);
                base.ClearItems();
                return;
            }

            if (_lstBeforeRemoveAsync.Count > 0)
            {
                List<RemovingOldEventArgs> lstArgsList = new List<RemovingOldEventArgs>();
                for (int j = 0; j < Items.Count; ++j)
                    lstArgsList.Add(new RemovingOldEventArgs(Items[j], j));
                List<Task> lstTasks = new List<Task>(Utils.MaxParallelBatchSize);
                int i = 0;
                foreach (AsyncBeforeRemoveEventHandler objEvent in _lstBeforeRemoveAsync)
                {
                    foreach (RemovingOldEventArgs objArgs in lstArgsList)
                    {
                        lstTasks.Add(objEvent.Invoke(this, objArgs, token));
                        if (++i < Utils.MaxParallelBatchSize)
                            continue;
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        lstTasks.Clear();
                        i = 0;
                    }
                }

                await Task.WhenAll(lstTasks).ConfigureAwait(false);
            }

            RaiseListChangedEvents = false;
            try
            {
                foreach (T obj in Items)
                    await UnhookAsyncPropertyChangedAsync(obj, token).ConfigureAwait(false);
                base.ClearItems();
            }
            finally
            {
                RaiseListChangedEvents = true;
            }

            await FireListChangedAsync(ListChangedType.Reset, -1, token);
        }

        public Task<bool> ContainsAsync(T item, CancellationToken token = default)
        {
            return token.IsCancellationRequested
                ? Task.FromCanceled<bool>(token)
                : Task.FromResult(Contains(item));
        }

        public Task CopyToAsync(T[] array, int index, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            CopyTo(array, index);
            return Task.CompletedTask;
        }

        public async Task<bool> RemoveAsync(T item, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intIndex = IndexOf(item);
            if (intIndex < 0)
                return false;
            await RemoveAtAsync(intIndex, token).ConfigureAwait(false);
            return true;
        }

        public Task<T> GetValueAtAsync(int index, CancellationToken token)
        {
            return token.IsCancellationRequested
                ? Task.FromCanceled<T>(token)
                : Task.FromResult(this[index]);
        }

        public Task SetItemAsync(int index, T value, CancellationToken token = default) =>
            SetValueAtAsync(index, value, token);

        public async Task SetValueAtAsync(int index, T value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            T objOldItem = this[index];
            if (!RaiseListChangedEvents)
            {
                await UnhookAsyncPropertyChangedAsync(objOldItem, token).ConfigureAwait(false);
                await HookAsyncPropertyChangedAsync(value, token).ConfigureAwait(false);
                base.SetItem(index, value);
                return;
            }

            if (_lstBeforeRemoveAsync.Count > 0 && !ReferenceEquals(objOldItem, value))
            {
                RemovingOldEventArgs objArgs = new RemovingOldEventArgs(Items[index], index);
                List<Task> lstTasks = new List<Task>(Utils.MaxParallelBatchSize);
                int i = 0;
                foreach (AsyncBeforeRemoveEventHandler objEvent in _lstBeforeRemoveAsync)
                {
                    lstTasks.Add(objEvent.Invoke(this, objArgs, token));
                    if (++i < Utils.MaxParallelBatchSize)
                        continue;
                    await Task.WhenAll(lstTasks).ConfigureAwait(false);
                    lstTasks.Clear();
                    i = 0;
                }

                await Task.WhenAll(lstTasks).ConfigureAwait(false);
            }

            RaiseListChangedEvents = false;
            try
            {
                await UnhookAsyncPropertyChangedAsync(objOldItem, token).ConfigureAwait(false);
                await HookAsyncPropertyChangedAsync(value, token).ConfigureAwait(false);
                base.SetItem(index, value);
            }
            finally
            {
                RaiseListChangedEvents = true;
            }

            await FireListChangedAsync(ListChangedType.ItemChanged, index, token);
        }

        public Task<int> IndexOfAsync(T item, CancellationToken token = default)
        {
            return token.IsCancellationRequested
                ? Task.FromCanceled<int>(token)
                : Task.FromResult(IndexOf(item));
        }

        protected override void InsertItem(int index, T item)
        {
            HookAsyncPropertyChanged(item);
            base.InsertItem(index, item);
        }

        public Task InsertItemAsync(int index, T item, CancellationToken token = default) =>
            InsertAsync(index, item, token);

        public async Task InsertAsync(int index, T item, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!RaiseListChangedEvents)
            {
                await HookAsyncPropertyChangedAsync(item, token).ConfigureAwait(false);
                base.InsertItem(index, item);
                return;
            }

            RaiseListChangedEvents = false;
            try
            {
                await HookAsyncPropertyChangedAsync(item, token).ConfigureAwait(false);
                base.InsertItem(index, item);
            }
            finally
            {
                RaiseListChangedEvents = true;
            }

            await FireListChangedAsync(ListChangedType.ItemAdded, index, token);
        }

        public async Task RemoveAtAsync(int index, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!RaiseListChangedEvents)
            {
                await UnhookAsyncPropertyChangedAsync(this[index], token).ConfigureAwait(false);
                RemoveAt(index);
                return;
            }

            if (_lstBeforeRemoveAsync.Count > 0)
            {
                RemovingOldEventArgs objArgs = new RemovingOldEventArgs(Items[index], index);
                List<Task> lstTasks = new List<Task>(Utils.MaxParallelBatchSize);
                int i = 0;
                foreach (AsyncBeforeRemoveEventHandler objEvent in _lstBeforeRemoveAsync)
                {
                    lstTasks.Add(objEvent.Invoke(this, objArgs, token));
                    if (++i < Utils.MaxParallelBatchSize)
                        continue;
                    await Task.WhenAll(lstTasks).ConfigureAwait(false);
                    lstTasks.Clear();
                    i = 0;
                }

                await Task.WhenAll(lstTasks).ConfigureAwait(false);
            }

            RaiseListChangedEvents = false;
            try
            {
                await UnhookAsyncPropertyChangedAsync(this[index], token).ConfigureAwait(false);
                base.RemoveItem(index);
            }
            finally
            {
                RaiseListChangedEvents = true;
            }

            await FireListChangedAsync(ListChangedType.ItemDeleted, index, token);
        }

        public Task ResetBindingsAsync(CancellationToken token = default)
        {
            return FireListChangedAsync(ListChangedType.Reset, -1, token);
        }

        public Task ResetItemAsync(int position, CancellationToken token = default)
        {
            return FireListChangedAsync(ListChangedType.ItemChanged, position, token);
        }

        private void HookAsyncPropertyChanged(T item)
        {
            if (!(item is INotifyPropertyChangedAsync notifyPropertyChanged))
                return;

            IDisposable objLocker = BindingListLock?.EnterReadLock();
            try
            {
                if (propertyChangedAsyncEventHandler != null)
                {
                    notifyPropertyChanged.PropertyChangedAsync += propertyChangedAsyncEventHandler;
                    return;
                }
            }
            finally
            {
                objLocker?.Dispose();
            }

            objLocker = BindingListLock?.EnterUpgradeableReadLock();
            try
            {
                if (propertyChangedAsyncEventHandler == null)
                {
                    IDisposable objLocker2 = BindingListLock?.EnterWriteLock();
                    try
                    {
                        propertyChangedAsyncEventHandler = Child_PropertyChangedAsync;
                    }
                    finally
                    {
                        objLocker2?.Dispose();
                    }
                }

                notifyPropertyChanged.PropertyChangedAsync += propertyChangedAsyncEventHandler;
            }
            finally
            {
                objLocker?.Dispose();
            }
        }

        private async Task HookAsyncPropertyChangedAsync(T item, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (BindingListLock == null)
            {
                HookAsyncPropertyChanged(item);
                return;
            }
            if (!(item is INotifyPropertyChangedAsync notifyPropertyChanged))
                return;

            using (await BindingListLock.EnterReadLockAsync(token))
            {
                if (propertyChangedAsyncEventHandler != null)
                {
                    notifyPropertyChanged.PropertyChangedAsync += propertyChangedAsyncEventHandler;
                    return;
                }
            }

            IAsyncDisposable objLocker = await BindingListLock.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (propertyChangedAsyncEventHandler == null)
                {
                    IAsyncDisposable objLocker2 =
                        await BindingListLock.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        propertyChangedAsyncEventHandler = Child_PropertyChangedAsync;
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }

                notifyPropertyChanged.PropertyChangedAsync += propertyChangedAsyncEventHandler;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private void UnhookAsyncPropertyChanged(T item)
        {
            if (!(item is INotifyPropertyChangedAsync notifyPropertyChanged))
                return;
            IDisposable objLocker = BindingListLock?.EnterReadLock();
            try
            {
                if (propertyChangedAsyncEventHandler == null)
                    return;
                notifyPropertyChanged.PropertyChangedAsync -= propertyChangedAsyncEventHandler;
            }
            finally
            {
                objLocker?.Dispose();
            }
        }

        private async Task UnhookAsyncPropertyChangedAsync(T item, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (BindingListLock == null)
            {
                UnhookAsyncPropertyChanged(item);
                return;
            }
            if (!(item is INotifyPropertyChangedAsync notifyPropertyChanged))
                return;
            using (await BindingListLock.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (propertyChangedAsyncEventHandler == null)
                    return;
                notifyPropertyChanged.PropertyChangedAsync -= propertyChangedAsyncEventHandler;
            }
        }

        private async Task Child_PropertyChangedAsync(object sender, PropertyChangedEventArgs e,
            CancellationToken token = default)
        {
            IDisposable objReadLocker = BindingListLock != null
                ? await BindingListLock.EnterReadLockAsync(token).ConfigureAwait(false)
                : null;
            try
            {
                token.ThrowIfCancellationRequested();
                if (!RaiseListChangedEvents)
                    return;
            }
            finally
            {
                objReadLocker?.Dispose();
            }

            if (sender != null && e != null && !string.IsNullOrEmpty(e.PropertyName))
            {
                T obj;
                try
                {
                    obj = (T)sender;
                }
                catch (InvalidCastException)
                {
                    await ResetBindingsAsync(token).ConfigureAwait(false);
                    return;
                }

                IDisposable objLocker = BindingListLock != null
                    ? await BindingListLock.EnterReadLockAsync(token).ConfigureAwait(false)
                    : null;
                try
                {
                    token.ThrowIfCancellationRequested();
                    int num = _intLastAsyncChangeIndex;
                    if (num >= 0 && num < await GetCountAsync(token).ConfigureAwait(false) &&
                        (await GetValueAtAsync(num, token).ConfigureAwait(false)).Equals(obj))
                    {
                        if (itemTypeProperties == null)
                            itemTypeProperties = TypeDescriptor.GetProperties(typeof(T));
                        PropertyDescriptor propDesc = itemTypeProperties.Find(e.PropertyName, true);
                        await OnListChangedAsync(new ListChangedEventArgs(ListChangedType.ItemChanged, num, propDesc),
                            token).ConfigureAwait(false);
                        return;
                    }
                }
                finally
                {
                    objLocker?.Dispose();
                }

                IAsyncDisposable objLocker2 = BindingListLock != null
                    ? await BindingListLock.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false)
                    : null;
                try
                {
                    token.ThrowIfCancellationRequested();
                    int num = _intLastAsyncChangeIndex;
                    if (num < 0 || num >= await GetCountAsync(token).ConfigureAwait(false) ||
                        !(await GetValueAtAsync(num, token).ConfigureAwait(false)).Equals(obj))
                    {
                        num = await IndexOfAsync(obj, token).ConfigureAwait(false);
                        IAsyncDisposable objLocker3 = BindingListLock != null
                            ? await BindingListLock.EnterWriteLockAsync(token).ConfigureAwait(false)
                            : null;
                        try
                        {
                            _intLastAsyncChangeIndex = num;
                        }
                        finally
                        {
                            if (objLocker3 != null)
                                await objLocker3.DisposeAsync().ConfigureAwait(false);
                        }
                    }

                    if (num == -1)
                    {
                        await UnhookAsyncPropertyChangedAsync(obj, token).ConfigureAwait(false);
                        await ResetBindingsAsync(token).ConfigureAwait(false);
                        return;
                    }

                    if (itemTypeProperties == null)
                        itemTypeProperties = TypeDescriptor.GetProperties(typeof(T));
                    PropertyDescriptor propDesc = itemTypeProperties.Find(e.PropertyName, true);
                    await OnListChangedAsync(new ListChangedEventArgs(ListChangedType.ItemChanged, num, propDesc),
                        token).ConfigureAwait(false);
                }
                finally
                {
                    if (objLocker2 != null)
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                return;
            }

            await ResetBindingsAsync(token).ConfigureAwait(false);
        }

        private Task FireListChangedAsync(ListChangedType type, int index, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            return RaiseListChangedEvents
                ? OnListChangedAsync(new ListChangedEventArgs(type, index), token)
                : Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (T obj in Items)
                    UnhookAsyncPropertyChanged(obj);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            foreach (T obj in Items)
                await UnhookAsyncPropertyChangedAsync(obj).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }
    }

    public class RemovingOldEventArgs : EventArgs
    {
        public RemovingOldEventArgs(object objOldObject = null, int intOldIndex = 0)
        {
            OldObject = objOldObject;
            OldIndex = intOldIndex;
        }

        public object OldObject { get; }
        public int OldIndex { get; }
    }

    public delegate Task AsyncBeforeRemoveEventHandler(object sender, RemovingOldEventArgs e, CancellationToken token = default);

    public delegate Task AsyncAddingNewEventHandler(object sender, AddingNewEventArgs e, CancellationToken token = default);

    public delegate Task AsyncListChangedEventHandler(object sender, ListChangedEventArgs e, CancellationToken token = default);
}
