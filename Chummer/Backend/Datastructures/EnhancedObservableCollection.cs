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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Chummer.Annotations;

namespace Chummer
{
    /// <summary>
    /// Expanded version of ObservableCollection that has an extra event for processing items before a Clear() command is executed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EnhancedObservableCollection<T> : ObservableCollection<T>, INotifyMultiplePropertyChangedAsync, IAsyncList<T>
    {
        /// <summary>
        /// CollectionChanged event subscription that will fire right before the collection is cleared.
        /// To make things easy, all of the collections elements will be present in e.OldItems.
        /// </summary>
        [SuppressMessage("Design", "CA1070:Do not declare event fields as virtual", Justification = "We do want to override this, actually. Just make sure that any override has explicit adders and removers defined.")]
        public virtual event NotifyCollectionChangedEventHandler BeforeClearCollectionChanged;

        private readonly List<AsyncNotifyCollectionChangedEventHandler> _lstBeforeClearCollectionChangedAsync =
            new List<AsyncNotifyCollectionChangedEventHandler>();

        /// <summary>
        /// CollectionChanged event subscription for async events that will fire right before the collection is cleared.
        /// To make things easy, all of the collections elements will be present in e.OldItems.
        /// Use this event instead of BeforeClearCollectionChanged for tasks that will be awaited before completion.
        /// </summary>
        [SuppressMessage("Design", "CA1070:Do not declare event fields as virtual", Justification = "We do want to override this, actually. Just make sure that any override has explicit adders and removers defined.")]
        public virtual event AsyncNotifyCollectionChangedEventHandler BeforeClearCollectionChangedAsync
        {
            add => _lstBeforeClearCollectionChangedAsync.Add(value);
            remove => _lstBeforeClearCollectionChangedAsync.Remove(value);
        }

        /// <inheritdoc />
        public EnhancedObservableCollection()
        {
        }

        /// <inheritdoc />
        public EnhancedObservableCollection(List<T> list) : base(list)
        {
        }

        /// <inheritdoc />
        public EnhancedObservableCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        private readonly List<PropertyChangedAsyncEventHandler> _lstPropertyChangedAsync =
            new List<PropertyChangedAsyncEventHandler>();

        public virtual event PropertyChangedAsyncEventHandler PropertyChangedAsync
        {
            add => _lstPropertyChangedAsync.Add(value);
            remove => _lstPropertyChangedAsync.Remove(value);
        }

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            this.OnMultiplePropertyChanged(strPropertyName);
        }

        public Task OnPropertyChangedAsync(string strPropertyName, CancellationToken token = default)
        {
            return this.OnMultiplePropertyChangedAsync(token, strPropertyName);
        }

        public void OnMultiplePropertyChanged(IReadOnlyCollection<string> lstPropertyNames)
        {
            if (_lstPropertyChangedAsync.Count > 0)
            {
                List<PropertyChangedEventArgs> lstArgsList = lstPropertyNames.Select(x => new PropertyChangedEventArgs(x)).ToList();
                Func<Task>[] aFuncs = new Func<Task>[lstArgsList.Count * _lstPropertyChangedAsync.Count];
                int i = 0;
                foreach (PropertyChangedAsyncEventHandler objEvent in _lstPropertyChangedAsync)
                {
                    foreach (PropertyChangedEventArgs objArg in lstArgsList)
                        aFuncs[i++] = () => objEvent.Invoke(this, objArg);
                }

                Utils.RunWithoutThreadLock(aFuncs);
                Utils.RunOnMainThread(() =>
                {
                    if (PropertyChanged != null)
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        foreach (PropertyChangedEventArgs objArgs in lstArgsList)
                        {
                            PropertyChanged.Invoke(this, objArgs);
                            base.OnPropertyChanged(objArgs);
                        }
                    }
                    else
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        foreach (PropertyChangedEventArgs objArgs in lstArgsList)
                        {
                            base.OnPropertyChanged(objArgs);
                        }
                    }
                });
            }
            else
            {
                Utils.RunOnMainThread(() =>
                {
                    if (PropertyChanged != null)
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        foreach (string strPropertyToChange in lstPropertyNames)
                        {
                            PropertyChangedEventArgs objArgs = new PropertyChangedEventArgs(strPropertyToChange);
                            PropertyChanged.Invoke(this, objArgs);
                            base.OnPropertyChanged(objArgs);
                        }
                    }
                    else
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        foreach (string strPropertyToChange in lstPropertyNames)
                        {
                            base.OnPropertyChanged(new PropertyChangedEventArgs(strPropertyToChange));
                        }
                    }
                });
            }
        }

        public async Task OnMultiplePropertyChangedAsync(IReadOnlyCollection<string> lstPropertyNames, CancellationToken token = default)
        {
            if (_lstPropertyChangedAsync.Count > 0)
            {
                List<PropertyChangedEventArgs> lstArgsList = lstPropertyNames
                    .Select(x => new PropertyChangedEventArgs(x)).ToList();
                List<Task> lstTasks = new List<Task>(Utils.MaxParallelBatchSize);
                int i = 0;
                foreach (PropertyChangedAsyncEventHandler objEvent in _lstPropertyChangedAsync)
                {
                    foreach (PropertyChangedEventArgs objArg in lstArgsList)
                    {
                        lstTasks.Add(objEvent.Invoke(this, objArg, token));
                        if (++i < Utils.MaxParallelBatchSize)
                            continue;
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        lstTasks.Clear();
                        i = 0;
                    }
                }

                await Task.WhenAll(lstTasks).ConfigureAwait(false);
                await Utils.RunOnMainThreadAsync(() =>
                {
                    if (PropertyChanged != null)
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        foreach (PropertyChangedEventArgs objArgs in lstArgsList)
                        {
                            PropertyChanged.Invoke(this, objArgs);
                            base.OnPropertyChanged(objArgs);
                        }
                    }
                    else
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        foreach (PropertyChangedEventArgs objArgs in lstArgsList)
                        {
                            base.OnPropertyChanged(objArgs);
                        }
                    }
                }, token).ConfigureAwait(false);
            }
            else
            {
                await Utils.RunOnMainThreadAsync(() =>
                {
                    if (PropertyChanged != null)
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        foreach (string strPropertyToChange in lstPropertyNames)
                        {
                            PropertyChangedEventArgs objArgs = new PropertyChangedEventArgs(strPropertyToChange);
                            PropertyChanged.Invoke(this, objArgs);
                            base.OnPropertyChanged(objArgs);
                        }
                    }
                    else
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        foreach (string strPropertyToChange in lstPropertyNames)
                        {
                            base.OnPropertyChanged(new PropertyChangedEventArgs(strPropertyToChange));
                        }
                    }
                }, token: token).ConfigureAwait(false);
            }
        }

        private Task OnCollectionChangedAsync(NotifyCollectionChangedAction action, object item, int index, CancellationToken token = default)
        {
            return OnCollectionChangedAsync(new NotifyCollectionChangedEventArgs(action, item, index), token);
        }

        private Task OnCollectionChangedAsync(NotifyCollectionChangedAction action, object item, int index, int oldIndex, CancellationToken token = default)
        {
            return OnCollectionChangedAsync(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex), token);
        }

        private Task OnCollectionChangedAsync(NotifyCollectionChangedAction action, object oldItem, object newItem, int index, CancellationToken token = default)
        {
            return OnCollectionChangedAsync(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index), token);
        }

        private Task OnCollectionResetAsync(CancellationToken token = default)
        {
            return OnCollectionChangedAsync(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset), token);
        }

        public Task MoveAsync(int oldIndex, int newIndex, CancellationToken token = default) => MoveItemAsync(oldIndex, newIndex, token);

        /// <inheritdoc />
        protected override void ClearItems()
        {
            if (_lstBeforeClearCollectionChangedAsync.Count != 0)
            {
                Utils.SafelyRunSynchronously(async () =>
                {
                    IDisposable objLocker = null;
                    if (CollectionChangedLock != null)
                        objLocker = await CollectionChangedLock.EnterReadLockAsync().ConfigureAwait(false);
                    try
                    {
                        using (BlockReentrancy())
                        {
                            NotifyCollectionChangedEventArgs objArgs =
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                                    (IList)Items);
                            await Task.WhenAll(
                                _lstBeforeClearCollectionChangedAsync.Select(x => x.Invoke(this, objArgs))).ConfigureAwait(false);
                            BeforeClearCollectionChanged?.Invoke(this, objArgs);
                        }
                    }
                    finally
                    {
                        objLocker?.Dispose();
                    }
                });
            }
            else
            {
                IDisposable objLocker = CollectionChangedLock?.EnterReadLock();
                try
                {
                    using (BlockReentrancy())
                    {
                        BeforeClearCollectionChanged?.Invoke(this,
                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (IList)Items));
                    }
                }
                finally
                {
                    objLocker?.Dispose();
                }
            }

            base.ClearItems();
        }

        public virtual async Task ClearItemsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CheckReentrancy();
            IDisposable objLocker = null;
            if (CollectionChangedLock != null)
                objLocker = await CollectionChangedLock.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_lstBeforeClearCollectionChangedAsync.Count != 0)
                {

                    using (BlockReentrancy())
                    {
                        NotifyCollectionChangedEventArgs objArgs =
                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                                (IList)Items);
                        await Task.WhenAll(
                                _lstBeforeClearCollectionChangedAsync.Select(x => x.Invoke(this, objArgs, token)))
                            .ConfigureAwait(false);
                        BeforeClearCollectionChanged?.Invoke(this, objArgs);
                    }
                }
                else
                {
                    using (BlockReentrancy())
                    {
                        BeforeClearCollectionChanged?.Invoke(this,
                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (IList)Items));
                    }
                }
            }
            finally
            {
                objLocker?.Dispose();
            }

            Items.Clear();
            await this.OnMultiplePropertyChangedAsync(token, "Count", "Item[]").ConfigureAwait(false);
            await OnCollectionResetAsync(token).ConfigureAwait(false);
        }

        public virtual async Task RemoveItemAsync(int index, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CheckReentrancy();
            T obj = this[index];
            Items.RemoveAt(index);
            await this.OnMultiplePropertyChangedAsync(token, "Count", "Item[]").ConfigureAwait(false);
            await OnCollectionChangedAsync(NotifyCollectionChangedAction.Remove, obj, index, token).ConfigureAwait(false);
        }

        public virtual async Task InsertItemAsync(int index, T item, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CheckReentrancy();
            Items.Insert(index, item);
            await this.OnMultiplePropertyChangedAsync(token, "Count", "Item[]").ConfigureAwait(false);
            await OnCollectionChangedAsync(NotifyCollectionChangedAction.Add, item, index, token).ConfigureAwait(false);
        }

        public virtual async Task SetItemAsync(int index, T item, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CheckReentrancy();
            T oldItem = Items[index];
            Items[index] = item;
            await OnPropertyChangedAsync("Item[]", token).ConfigureAwait(false);
            await OnCollectionChangedAsync(NotifyCollectionChangedAction.Replace, oldItem, item, index, token).ConfigureAwait(false);
        }

        public virtual async Task MoveItemAsync(int oldIndex, int newIndex, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CheckReentrancy();
            T obj = Items[oldIndex];
            Items.RemoveAt(oldIndex);
            Items.Insert(newIndex, obj);
            await OnPropertyChangedAsync("Item[]", token).ConfigureAwait(false);
            await OnCollectionChangedAsync(NotifyCollectionChangedAction.Move, obj, newIndex, oldIndex, token).ConfigureAwait(false);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_lstCollectionChangedAsync.Count != 0)
            {
                Utils.SafelyRunSynchronously(async () =>
                {
                    IDisposable objLocker = null;
                    if (CollectionChangedLock != null)
                        objLocker = await CollectionChangedLock.EnterReadLockAsync().ConfigureAwait(false);
                    try
                    {
                        await Task.WhenAll(
                            _lstCollectionChangedAsync.Select(x => x.Invoke(this, e))).ConfigureAwait(false);
                        base.OnCollectionChanged(e);
                    }
                    finally
                    {
                        objLocker?.Dispose();
                    }
                });
            }
            else
            {
                IDisposable objLocker = CollectionChangedLock?.EnterReadLock();
                try
                {
                    base.OnCollectionChanged(e);
                }
                finally
                {
                    objLocker?.Dispose();
                }
            }
        }

        protected virtual async Task OnCollectionChangedAsync(NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_lstCollectionChangedAsync.Count != 0)
            {
                IDisposable objLocker = null;
                if (CollectionChangedLock != null)
                    objLocker = await CollectionChangedLock.EnterReadLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    await Task.WhenAll(_lstCollectionChangedAsync.Select(x => x.Invoke(this, e, token))).ConfigureAwait(false);
                    base.OnCollectionChanged(e);
                }
                finally
                {
                    objLocker?.Dispose();
                }
            }
            else
            {
                IDisposable objLocker = CollectionChangedLock != null ? await CollectionChangedLock.EnterReadLockAsync(token).ConfigureAwait(false) : null;
                try
                {
                    token.ThrowIfCancellationRequested();
                    base.OnCollectionChanged(e);
                }
                finally
                {
                    objLocker?.Dispose();
                }
            }
        }

        public AsyncFriendlyReaderWriterLock CollectionChangedLock { get; set; }

        private readonly List<AsyncNotifyCollectionChangedEventHandler> _lstCollectionChangedAsync =
            new List<AsyncNotifyCollectionChangedEventHandler>();

        /// <summary>
        /// Like CollectionChanged, occurs when an item is added, removed, changed, moved, or the entire list is refreshed.
        /// Use this event instead of CollectionChanged for tasks that will be awaited before completion.
        /// </summary>
        [SuppressMessage("Design", "CA1070:Do not declare event fields as virtual", Justification = "We do want to override this, actually. Just make sure that any override has explicit adders and removers defined.")]
        public virtual event AsyncNotifyCollectionChangedEventHandler CollectionChangedAsync
        {
            add => _lstCollectionChangedAsync.Add(value);
            remove => _lstCollectionChangedAsync.Remove(value);
        }

        public Task<IEnumerator<T>> GetEnumeratorAsync(CancellationToken token = default)
        {
            return token.IsCancellationRequested ? Task.FromCanceled<IEnumerator<T>>(token) : Task.FromResult(GetEnumerator());
        }

        public Task<int> GetCountAsync(CancellationToken token = default)
        {
            return token.IsCancellationRequested ? Task.FromCanceled<int>(token) : Task.FromResult(Count);
        }

        public async Task AddAsync(T item, CancellationToken token = default)
        {
            await InsertItemAsync(await GetCountAsync(token).ConfigureAwait(false), item, token).ConfigureAwait(false);
        }

        public Task ClearAsync(CancellationToken token = default)
        {
            return ClearItemsAsync(token);
        }

        public Task<bool> ContainsAsync(T item, CancellationToken token = default)
        {
            return token.IsCancellationRequested ? Task.FromCanceled<bool>(token) : Task.FromResult(Contains(item));
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
            int index = await IndexOfAsync(item, token).ConfigureAwait(false);
            if (index < 0)
                return false;
            await RemoveItemAsync(index, token).ConfigureAwait(false);
            return true;
        }

        public Task<T> GetValueAtAsync(int index, CancellationToken token = default)
        {
            return token.IsCancellationRequested ? Task.FromCanceled<T>(token) : Task.FromResult(this[index]);
        }

        public Task SetValueAtAsync(int index, T value, CancellationToken token = default)
        {
            return SetItemAsync(index, value, token);
        }

        public Task<int> IndexOfAsync(T item, CancellationToken token = default)
        {
            return token.IsCancellationRequested ? Task.FromCanceled<int>(token) : Task.FromResult(IndexOf(item));
        }

        public Task InsertAsync(int index, T item, CancellationToken token = default)
        {
            return InsertItemAsync(index, item, token);
        }

        public Task RemoveAtAsync(int index, CancellationToken token = default)
        {
            return RemoveItemAsync(index, token);
        }
    }

    public delegate Task AsyncNotifyCollectionChangedEventHandler(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default);
}
