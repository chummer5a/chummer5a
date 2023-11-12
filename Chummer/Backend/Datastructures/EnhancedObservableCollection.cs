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
using System.Linq;
using System.Threading.Tasks;

namespace Chummer
{
    /// <summary>
    /// Expanded version of ObservableCollection that has an extra event for processing items before a Clear() command is executed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EnhancedObservableCollection<T> : ObservableCollection<T>
    {
        /// <summary>
        /// CollectionChanged event subscription that will fire right before the collection is cleared.
        /// To make things easy, all of the collections elements will be present in e.OldItems.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1070:Do not declare event fields as virtual", Justification = "We do want to override this, actually. Just make sure that any override has explicit adders and removers defined.")]
        public virtual event NotifyCollectionChangedEventHandler BeforeClearCollectionChanged;

        private readonly List<AsyncNotifyCollectionChangedEventHandler> _lstBeforeClearCollectionChangedAsync =
            new List<AsyncNotifyCollectionChangedEventHandler>();

        /// <summary>
        /// CollectionChanged event subscription for async events that will fire right before the collection is cleared.
        /// To make things easy, all of the collections elements will be present in e.OldItems.
        /// Use this event instead of BeforeClearCollectionChanged for tasks that will be awaited before completion.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1070:Do not declare event fields as virtual", Justification = "We do want to override this, actually. Just make sure that any override has explicit adders and removers defined.")]
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
                                _lstBeforeClearCollectionChangedAsync.Select(x => x.Invoke(this, objArgs)));
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
                            _lstCollectionChangedAsync.Select(x => x.Invoke(this, e)));
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

        public AsyncFriendlyReaderWriterLock CollectionChangedLock { get; set; }

        private readonly List<AsyncNotifyCollectionChangedEventHandler> _lstCollectionChangedAsync =
            new List<AsyncNotifyCollectionChangedEventHandler>();

        /// <summary>
        /// Like CollectionChanged, occurs when an item is added, removed, changed, moved, or the entire list is refreshed.
        /// Use this event instead of CollectionChanged for tasks that will be awaited before completion.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1070:Do not declare event fields as virtual", Justification = "We do want to override this, actually. Just make sure that any override has explicit adders and removers defined.")]
        public virtual event AsyncNotifyCollectionChangedEventHandler CollectionChangedAsync
        {
            add => _lstCollectionChangedAsync.Add(value);
            remove => _lstCollectionChangedAsync.Remove(value);
        }
    }

    public delegate Task AsyncNotifyCollectionChangedEventHandler(object sender, NotifyCollectionChangedEventArgs e);
}
