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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    public class CachedBindingList<T> : BindingList<T>
    {
        public AsyncFriendlyReaderWriterLock BindingListLock { get; set; }

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
                if (_lstBeforeRemoveAsync.Count > 0)
                {
                    Utils.SafelyRunSynchronously(async () =>
                    {
                        IDisposable objLocker = null;
                        if (BindingListLock != null)
                            objLocker = await BindingListLock.EnterReadLockAsync().ConfigureAwait(false);
                        try
                        {
                            RemovingOldEventArgs objArgs = new RemovingOldEventArgs(Items[index], index);
                            await Task.WhenAll(
                                _lstBeforeRemoveAsync.Select(x => x.Invoke(this, objArgs)));
                            BeforeRemove?.Invoke(this, objArgs);
                        }
                        finally
                        {
                            objLocker?.Dispose();
                        }
                    });
                }
                else if (BeforeRemove != null)
                {
                    IDisposable objLocker = BindingListLock?.EnterReadLock();
                    try
                    {
                        BeforeRemove?.Invoke(this, new RemovingOldEventArgs(Items[index], index));
                    }
                    finally
                    {
                        objLocker?.Dispose();
                    }
                }
            }

            base.RemoveItem(index);
        }

        /// <inheritdoc />
        protected override void ClearItems()
        {
            if (RaiseListChangedEvents)
            {
                if (_lstBeforeRemoveAsync.Count > 0)
                {
                    Utils.SafelyRunSynchronously(async () =>
                    {
                        IDisposable objLocker = null;
                        if (BindingListLock != null)
                            objLocker = await BindingListLock.EnterReadLockAsync().ConfigureAwait(false);
                        try
                        {
                            for (int i = 0; i < Items.Count; ++i)
                            {
                                RemovingOldEventArgs objArgs = new RemovingOldEventArgs(Items[i], i);
                                await Task.WhenAll(
                                    _lstBeforeRemoveAsync.Select(x => x.Invoke(this, objArgs)));
                                BeforeRemove?.Invoke(this, objArgs);
                            }
                            
                        }
                        finally
                        {
                            objLocker?.Dispose();
                        }
                    });
                }
                else if (BeforeRemove != null)
                {
                    for (int i = 0; i < Items.Count; ++i)
                    {
                        BeforeRemove.Invoke(this, new RemovingOldEventArgs(Items[i], i));
                    }
                }
            }

            base.ClearItems();
        }

        /// <inheritdoc />
        protected override void SetItem(int index, T item)
        {
            if (RaiseListChangedEvents)
            {
                T objOldItem = Items[index];
                if (!ReferenceEquals(objOldItem, item))
                {
                    BeforeRemove?.Invoke(this, new RemovingOldEventArgs(objOldItem, index));
                }
            }

            base.SetItem(index, item);
        }

        /// <inheritdoc />
        protected override void OnAddingNew(AddingNewEventArgs e)
        {
            if (_lstAddingNewAsync.Count > 0)
            {
                Utils.SafelyRunSynchronously(async () =>
                {
                    IDisposable objLocker = null;
                    if (BindingListLock != null)
                        objLocker = await BindingListLock.EnterReadLockAsync().ConfigureAwait(false);
                    try
                    {
                        await Task.WhenAll(_lstAddingNewAsync.Select(x => x.Invoke(this, e)));
                        base.OnAddingNew(e);
                    }
                    finally
                    {
                        objLocker?.Dispose();
                    }
                });
            }
            else
            {
                IDisposable objLocker = BindingListLock?.EnterReadLock();
                try
                {
                    base.OnAddingNew(e);
                }
                finally
                {
                    objLocker?.Dispose();
                }
            }
        }

        /// <inheritdoc />
        protected override void OnListChanged(ListChangedEventArgs e)
        {
            if (_lstListChangedAsync.Count > 0)
            {
                Utils.SafelyRunSynchronously(async () =>
                {
                    IDisposable objLocker = null;
                    if (BindingListLock != null)
                        objLocker = await BindingListLock.EnterReadLockAsync().ConfigureAwait(false);
                    try
                    {
                        await Task.WhenAll(_lstListChangedAsync.Select(x => x.Invoke(this, e)));
                        base.OnListChanged(e);
                    }
                    finally
                    {
                        objLocker?.Dispose();
                    }
                });
            }
            else
            {
                IDisposable objLocker = BindingListLock?.EnterReadLock();
                try
                {
                    base.OnListChanged(e);
                }
                finally
                {
                    objLocker?.Dispose();
                }
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
