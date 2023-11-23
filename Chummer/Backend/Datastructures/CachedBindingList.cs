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
    public class CachedBindingList<T> : BindingList<T>, IAsyncList<T>, IAsyncReadOnlyList<T>
    {
        public AsyncFriendlyReaderWriterLock BindingListLock { get; set; }

        private bool _blnDoAsyncEventHandlers;

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
        }

        /// <inheritdoc />
        protected override void RemoveItem(int index)
        {
            if (RaiseListChangedEvents)
            {
                IDisposable objLocker = BindingListLock?.EnterReadLock();
                try
                {
                    if (_blnDoAsyncEventHandlers && _lstBeforeRemoveAsync.Count > 0)
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
                    if (_blnDoAsyncEventHandlers && _lstBeforeRemoveAsync.Count > 0)
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
                        if (_blnDoAsyncEventHandlers && _lstBeforeRemoveAsync.Count > 0)
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

            base.SetItem(index, item);
        }

        /// <inheritdoc />
        protected override void OnAddingNew(AddingNewEventArgs e)
        {
            IDisposable objLocker = BindingListLock?.EnterReadLock();
            try
            {
                if (_blnDoAsyncEventHandlers && _lstAddingNewAsync.Count > 0)
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
                if (_blnDoAsyncEventHandlers && _lstListChangedAsync.Count > 0)
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
            token.ThrowIfCancellationRequested();
            if (!RaiseListChangedEvents)
            {
                Add(item);
                return;
            }

            bool blnOldDoAsyncEventHandlers = _blnDoAsyncEventHandlers;
            try
            {
                _blnDoAsyncEventHandlers = false;
                Add(item);
                if (_lstListChangedAsync.Count != 0)
                {
                    ListChangedEventArgs objArg = new ListChangedEventArgs(ListChangedType.ItemAdded, Count - 1);
                    IDisposable objLocker = null;
                    if (BindingListLock != null)
                        objLocker = await BindingListLock.EnterReadLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        List<Task> lstTasks = new List<Task>(Utils.MaxParallelBatchSize);
                        int i = 0;
                        foreach (AsyncListChangedEventHandler objEvent in _lstListChangedAsync)
                        {
                            lstTasks.Add(objEvent.Invoke(this, objArg, token));
                            if (++i < Utils.MaxParallelBatchSize)
                                continue;
                            await Task.WhenAll(lstTasks).ConfigureAwait(false);
                            lstTasks.Clear();
                            i = 0;
                        }

                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                    }
                    finally
                    {
                        objLocker?.Dispose();
                    }
                }
            }
            finally
            {
                _blnDoAsyncEventHandlers = blnOldDoAsyncEventHandlers;
            }
        }

        public async Task ClearAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!RaiseListChangedEvents)
            {
                Clear();
                return;
            }

            if (_blnDoAsyncEventHandlers && _lstBeforeRemoveAsync.Count > 0)
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

            bool blnOldDoAsyncEventHandlers = _blnDoAsyncEventHandlers;
            try
            {
                _blnDoAsyncEventHandlers = false;
                Clear();
                if (_lstListChangedAsync.Count != 0)
                {
                    ListChangedEventArgs objArgs = new ListChangedEventArgs(ListChangedType.Reset, -1);
                    IDisposable objLocker = null;
                    if (BindingListLock != null)
                        objLocker = await BindingListLock.EnterReadLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        List<Task> lstTasks = new List<Task>(Utils.MaxParallelBatchSize);
                        int i = 0;
                        foreach (AsyncListChangedEventHandler objEvent in _lstListChangedAsync)
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
                    finally
                    {
                        objLocker?.Dispose();
                    }
                }
            }
            finally
            {
                _blnDoAsyncEventHandlers = blnOldDoAsyncEventHandlers;
            }
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

        public async Task SetValueAtAsync(int index, T value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!RaiseListChangedEvents)
            {
                this[index] = value;
                return;
            }

            T objOldItem = this[index];
            if (!ReferenceEquals(objOldItem, value))
            {
                if (_blnDoAsyncEventHandlers && _lstBeforeRemoveAsync.Count > 0)
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
            }

            bool blnOldDoAsyncEventHandlers = _blnDoAsyncEventHandlers;
            try
            {
                _blnDoAsyncEventHandlers = false;
                this[index] = value;
                if (_lstListChangedAsync.Count != 0)
                {
                    ListChangedEventArgs objArg = new ListChangedEventArgs(ListChangedType.ItemChanged, index);
                    IDisposable objLocker = null;
                    if (BindingListLock != null)
                        objLocker = await BindingListLock.EnterReadLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        List<Task> lstTasks = new List<Task>(Utils.MaxParallelBatchSize);
                        int i = 0;
                        foreach (AsyncListChangedEventHandler objEvent in _lstListChangedAsync)
                        {
                            lstTasks.Add(objEvent.Invoke(this, objArg, token));
                            if (++i < Utils.MaxParallelBatchSize)
                                continue;
                            await Task.WhenAll(lstTasks).ConfigureAwait(false);
                            lstTasks.Clear();
                            i = 0;
                        }

                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                    }
                    finally
                    {
                        objLocker?.Dispose();
                    }
                }
            }
            finally
            {
                _blnDoAsyncEventHandlers = blnOldDoAsyncEventHandlers;
            }
        }

        public Task<int> IndexOfAsync(T item, CancellationToken token = default)
        {
            return token.IsCancellationRequested
                ? Task.FromCanceled<int>(token)
                : Task.FromResult(IndexOf(item));
        }

        public async Task InsertAsync(int index, T item, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!RaiseListChangedEvents)
            {
                Insert(index, item);
                return;
            }

            bool blnOldDoAsyncEventHandlers = _blnDoAsyncEventHandlers;
            try
            {
                _blnDoAsyncEventHandlers = false;
                Insert(index, item);
                if (_lstListChangedAsync.Count != 0)
                {
                    ListChangedEventArgs objArg = new ListChangedEventArgs(ListChangedType.ItemAdded, index);
                    IDisposable objLocker = null;
                    if (BindingListLock != null)
                        objLocker = await BindingListLock.EnterReadLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        List<Task> lstTasks = new List<Task>(Utils.MaxParallelBatchSize);
                        int i = 0;
                        foreach (AsyncListChangedEventHandler objEvent in _lstListChangedAsync)
                        {
                            lstTasks.Add(objEvent.Invoke(this, objArg, token));
                            if (++i < Utils.MaxParallelBatchSize)
                                continue;
                            await Task.WhenAll(lstTasks).ConfigureAwait(false);
                            lstTasks.Clear();
                            i = 0;
                        }

                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                    }
                    finally
                    {
                        objLocker?.Dispose();
                    }
                }
            }
            finally
            {
                _blnDoAsyncEventHandlers = blnOldDoAsyncEventHandlers;
            }
        }

        public async Task RemoveAtAsync(int index, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!RaiseListChangedEvents)
            {
                RemoveAt(index);
                return;
            }

            if (_blnDoAsyncEventHandlers && _lstBeforeRemoveAsync.Count > 0)
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

            bool blnOldDoAsyncEventHandlers = _blnDoAsyncEventHandlers;
            try
            {
                _blnDoAsyncEventHandlers = false;
                RemoveAt(index);
                if (_lstListChangedAsync.Count != 0)
                {
                    ListChangedEventArgs objArgs = new ListChangedEventArgs(ListChangedType.ItemDeleted, index);
                    IDisposable objLocker = null;
                    if (BindingListLock != null)
                        objLocker = await BindingListLock.EnterReadLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        List<Task> lstTasks = new List<Task>(Utils.MaxParallelBatchSize);
                        int i = 0;
                        foreach (AsyncListChangedEventHandler objEvent in _lstListChangedAsync)
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
                    finally
                    {
                        objLocker?.Dispose();
                    }
                }
            }
            finally
            {
                _blnDoAsyncEventHandlers = blnOldDoAsyncEventHandlers;
            }
        }

        public async Task ResetBindingsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!RaiseListChangedEvents)
            {
                ResetBindings();
                return;
            }

            bool blnOldDoAsyncEventHandlers = _blnDoAsyncEventHandlers;
            try
            {
                _blnDoAsyncEventHandlers = false;
                ResetBindings();
                if (_lstListChangedAsync.Count != 0)
                {
                    ListChangedEventArgs objArgs = new ListChangedEventArgs(ListChangedType.Reset, -1);
                    IDisposable objLocker = null;
                    if (BindingListLock != null)
                        objLocker = await BindingListLock.EnterReadLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        List<Task> lstTasks = new List<Task>(Utils.MaxParallelBatchSize);
                        int i = 0;
                        foreach (AsyncListChangedEventHandler objEvent in _lstListChangedAsync)
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
                    finally
                    {
                        objLocker?.Dispose();
                    }
                }
            }
            finally
            {
                _blnDoAsyncEventHandlers = blnOldDoAsyncEventHandlers;
            }
        }

        public async Task ResetItemAsync(int position, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!RaiseListChangedEvents)
            {
                ResetItem(position);
                return;
            }

            bool blnOldDoAsyncEventHandlers = _blnDoAsyncEventHandlers;
            try
            {
                _blnDoAsyncEventHandlers = false;
                ResetItem(position);
                if (_lstListChangedAsync.Count != 0)
                {
                    ListChangedEventArgs objArgs = new ListChangedEventArgs(ListChangedType.ItemChanged, position);
                    IDisposable objLocker = null;
                    if (BindingListLock != null)
                        objLocker = await BindingListLock.EnterReadLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        List<Task> lstTasks = new List<Task>(Utils.MaxParallelBatchSize);
                        int i = 0;
                        foreach (AsyncListChangedEventHandler objEvent in _lstListChangedAsync)
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
                    finally
                    {
                        objLocker?.Dispose();
                    }
                }
            }
            finally
            {
                _blnDoAsyncEventHandlers = blnOldDoAsyncEventHandlers;
            }
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
