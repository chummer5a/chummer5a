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
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    /// <inheritdoc />
    /// <summary>
    /// ObservableCollection that allows for adding and removal of anonymous delegates by an associated tag
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TaggedObservableCollection<T> : ThreadSafeObservableCollection<T>
    {
        private readonly ConcurrentDictionary<object, HashSet<NotifyCollectionChangedEventHandler>>
            _dicTaggedAddedDelegates = new ConcurrentDictionary<object, HashSet<NotifyCollectionChangedEventHandler>>();

        private readonly ConcurrentDictionary<object, HashSet<NotifyCollectionChangedEventHandler>>
            _dicTaggedAddedBeforeClearDelegates =
                new ConcurrentDictionary<object, HashSet<NotifyCollectionChangedEventHandler>>();

        private readonly ConcurrentDictionary<object, HashSet<AsyncNotifyCollectionChangedEventHandler>>
            _dicTaggedAddedAsyncDelegates = new ConcurrentDictionary<object, HashSet<AsyncNotifyCollectionChangedEventHandler>>();

        private readonly ConcurrentDictionary<object, HashSet<AsyncNotifyCollectionChangedEventHandler>>
            _dicTaggedAddedAsyncBeforeClearDelegates =
                new ConcurrentDictionary<object, HashSet<AsyncNotifyCollectionChangedEventHandler>>();

        /// <summary>
        /// Use in place of CollectionChanged Adder
        /// </summary>
        /// <param name="objTag">Tag to associate with added delegate</param>
        /// <param name="funcDelegateToAdd">Delegate to add to CollectionChanged</param>
        /// <returns>True if delegate was successfully added, false if a delegate already exists with the associated tag.</returns>
        public bool AddTaggedCollectionChanged(object objTag, NotifyCollectionChangedEventHandler funcDelegateToAdd)
        {
            HashSet<NotifyCollectionChangedEventHandler> setFuncs
                = _dicTaggedAddedDelegates.GetOrAdd(objTag, x => new HashSet<NotifyCollectionChangedEventHandler>());
            if (setFuncs.Add(funcDelegateToAdd))
            {
                base.CollectionChanged += funcDelegateToAdd;
                return true;
            }
            Utils.BreakIfDebug();
            return false;
        }

        /// <summary>
        /// Use in place of CollectionChanged Adder
        /// </summary>
        /// <param name="objTag">Tag to associate with added delegate</param>
        /// <param name="funcDelegateToAdd">Delegate to add to CollectionChanged</param>
        /// <param name="token">CancellationToken to listen to.</param>
        /// <returns>True if delegate was successfully added, false if a delegate already exists with the associated tag.</returns>
        public async Task<bool> AddTaggedCollectionChangedAsync(object objTag, NotifyCollectionChangedEventHandler funcDelegateToAdd, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                HashSet<NotifyCollectionChangedEventHandler> setFuncs
                    = _dicTaggedAddedDelegates.GetOrAdd(
                        objTag, x => new HashSet<NotifyCollectionChangedEventHandler>());

                if (setFuncs.Add(funcDelegateToAdd))
                {
                    base.CollectionChanged += funcDelegateToAdd;
                    return true;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            Utils.BreakIfDebug();
            return false;
        }

        /// <summary>
        /// Use in place of CollectionChanged Subtract
        /// </summary>
        /// <param name="objTag">Tag of delegate to remove from CollectionChanged</param>
        /// <param name="token">CancellationToken to listen to.</param>
        /// <returns>True if a delegate associated with the tag was found and deleted, false otherwise.</returns>
        public bool RemoveTaggedCollectionChanged(object objTag, CancellationToken token = default)
        {
            using (LockObject.EnterWriteLock(token))
            {
                if (!_dicTaggedAddedDelegates.TryGetValue(
                        objTag, out HashSet<NotifyCollectionChangedEventHandler> setFuncs))
                {
                    Utils.BreakIfDebug();
                    return false;
                }

                foreach (NotifyCollectionChangedEventHandler funcDelegateToRemove in setFuncs)
                {
                    base.CollectionChanged -= funcDelegateToRemove;
                }

                setFuncs.Clear();
                return true;
            }
        }

        /// <summary>
        /// Use in place of CollectionChanged Subtract
        /// </summary>
        /// <param name="objTag">Tag of delegate to remove from CollectionChanged</param>
        /// <param name="token">CancellationToken to listen to.</param>
        /// <returns>True if a delegate associated with the tag was found and deleted, false otherwise.</returns>
        public async Task<bool> RemoveTaggedCollectionChangedAsync(object objTag, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!_dicTaggedAddedDelegates.TryGetValue(
                    objTag, out HashSet<NotifyCollectionChangedEventHandler> setFuncs))
                {
                    Utils.BreakIfDebug();
                    return false;
                }

                foreach (NotifyCollectionChangedEventHandler funcDelegateToRemove in setFuncs)
                {
                    base.CollectionChanged -= funcDelegateToRemove;
                }

                setFuncs.Clear();
                return true;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Use in place of CollectionChangedAsync Adder
        /// </summary>
        /// <param name="objTag">Tag to associate with added delegate</param>
        /// <param name="funcDelegateToAdd">Delegate to add to CollectionChanged</param>
        /// <returns>True if delegate was successfully added, false if a delegate already exists with the associated tag.</returns>
        public bool AddTaggedCollectionChanged(object objTag, AsyncNotifyCollectionChangedEventHandler funcDelegateToAdd)
        {
            HashSet<AsyncNotifyCollectionChangedEventHandler> setFuncs
                = _dicTaggedAddedAsyncDelegates.GetOrAdd(objTag, x => new HashSet<AsyncNotifyCollectionChangedEventHandler>());
            if (setFuncs.Add(funcDelegateToAdd))
            {
                base.CollectionChangedAsync += funcDelegateToAdd;
                return true;
            }
            Utils.BreakIfDebug();
            return false;
        }

        /// <summary>
        /// Use in place of CollectionChangedAsync Adder
        /// </summary>
        /// <param name="objTag">Tag to associate with added delegate</param>
        /// <param name="funcDelegateToAdd">Delegate to add to CollectionChanged</param>
        /// <param name="token">CancellationToken to listen to.</param>
        /// <returns>True if delegate was successfully added, false if a delegate already exists with the associated tag.</returns>
        public async Task<bool> AddTaggedCollectionChangedAsync(object objTag, AsyncNotifyCollectionChangedEventHandler funcDelegateToAdd, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                HashSet<AsyncNotifyCollectionChangedEventHandler> setFuncs
                    = _dicTaggedAddedAsyncDelegates.GetOrAdd(
                        objTag, x => new HashSet<AsyncNotifyCollectionChangedEventHandler>());

                if (setFuncs.Add(funcDelegateToAdd))
                {
                    base.CollectionChangedAsync += funcDelegateToAdd;
                    return true;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            Utils.BreakIfDebug();
            return false;
        }

        /// <summary>
        /// Use in place of CollectionChangedAsync Subtract
        /// </summary>
        /// <param name="objTag">Tag of delegate to remove from CollectionChanged</param>
        /// <param name="token">CancellationToken to listen to.</param>
        /// <returns>True if a delegate associated with the tag was found and deleted, false otherwise.</returns>
        public bool RemoveTaggedAsyncCollectionChanged(object objTag, CancellationToken token = default)
        {
            using (LockObject.EnterWriteLock(token))
            {
                if (!_dicTaggedAddedAsyncDelegates.TryGetValue(
                        objTag, out HashSet<AsyncNotifyCollectionChangedEventHandler> setFuncs))
                {
                    Utils.BreakIfDebug();
                    return false;
                }

                foreach (AsyncNotifyCollectionChangedEventHandler funcDelegateToRemove in setFuncs)
                {
                    base.CollectionChangedAsync -= funcDelegateToRemove;
                }

                setFuncs.Clear();
                return true;
            }
        }

        /// <summary>
        /// Use in place of CollectionChangedAsync Subtract
        /// </summary>
        /// <param name="objTag">Tag of delegate to remove from CollectionChanged</param>
        /// <param name="token">CancellationToken to listen to.</param>
        /// <returns>True if a delegate associated with the tag was found and deleted, false otherwise.</returns>
        public async Task<bool> RemoveTaggedAsyncCollectionChangedAsync(object objTag, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!_dicTaggedAddedAsyncDelegates.TryGetValue(
                    objTag, out HashSet<AsyncNotifyCollectionChangedEventHandler> setFuncs))
                {
                    Utils.BreakIfDebug();
                    return false;
                }

                foreach (AsyncNotifyCollectionChangedEventHandler funcDelegateToRemove in setFuncs)
                {
                    base.CollectionChangedAsync -= funcDelegateToRemove;
                }

                setFuncs.Clear();
                return true;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public override event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => throw new NotSupportedException("TaggedObservableCollection should use AddTaggedCollectionChanged method instead of adding to CollectionChanged");
            remove => throw new NotSupportedException("TaggedObservableCollection should use RemoveTaggedCollectionChanged method instead of removing from CollectionChanged");
        }

        /// <inheritdoc />
        public override event AsyncNotifyCollectionChangedEventHandler CollectionChangedAsync
        {
            add => throw new NotSupportedException("TaggedObservableCollection should use AddTaggedCollectionChanged method instead of adding to CollectionChangedAsync");
            remove => throw new NotSupportedException("TaggedObservableCollection should use RemoveTaggedAsyncCollectionChanged method instead of removing from CollectionChangedAsync");
        }

        /// <summary>
        /// Use in place of CollectionChanged Adder
        /// </summary>
        /// <param name="objTag">Tag to associate with added delegate</param>
        /// <param name="funcDelegateToAdd">Delegate to add to CollectionChanged</param>
        /// <returns>True if delegate was successfully added, false if a delegate already exists with the associated tag.</returns>
        public bool AddTaggedBeforeClearCollectionChanged(object objTag, NotifyCollectionChangedEventHandler funcDelegateToAdd)
        {
            HashSet<NotifyCollectionChangedEventHandler> setFuncs
                = _dicTaggedAddedBeforeClearDelegates.GetOrAdd(
                    objTag, x => new HashSet<NotifyCollectionChangedEventHandler>());
            if (setFuncs.Add(funcDelegateToAdd))
            {
                base.CollectionChanged += funcDelegateToAdd;
                return true;
            }
            Utils.BreakIfDebug();
            return false;
        }

        /// <summary>
        /// Use in place of CollectionChanged Adder
        /// </summary>
        /// <param name="objTag">Tag to associate with added delegate</param>
        /// <param name="funcDelegateToAdd">Delegate to add to CollectionChanged</param>
        /// <param name="token">CancellationToken to listen to.</param>
        /// <returns>True if delegate was successfully added, false if a delegate already exists with the associated tag.</returns>
        public async Task<bool> AddTaggedBeforeClearCollectionChangedAsync(object objTag, NotifyCollectionChangedEventHandler funcDelegateToAdd, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                HashSet<NotifyCollectionChangedEventHandler> setFuncs
                    = _dicTaggedAddedBeforeClearDelegates.GetOrAdd(
                        objTag, x => new HashSet<NotifyCollectionChangedEventHandler>());

                if (setFuncs.Add(funcDelegateToAdd))
                {
                    base.BeforeClearCollectionChanged += funcDelegateToAdd;
                    return true;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            Utils.BreakIfDebug();
            return false;
        }

        /// <summary>
        /// Use in place of CollectionChanged Subtract
        /// </summary>
        /// <param name="objTag">Tag of delegate to remove from CollectionChanged</param>
        /// <param name="token">CancellationToken to listen to.</param>
        /// <returns>True if a delegate associated with the tag was found and deleted, false otherwise.</returns>
        public bool RemoveTaggedBeforeClearCollectionChanged(object objTag, CancellationToken token = default)
        {
            using (LockObject.EnterWriteLock(token))
            {
                if (!_dicTaggedAddedBeforeClearDelegates.TryGetValue(
                        objTag, out HashSet<NotifyCollectionChangedEventHandler> setFuncs))
                {
                    Utils.BreakIfDebug();
                    return false;
                }

                foreach (NotifyCollectionChangedEventHandler funcDelegateToRemove in setFuncs)
                {
                    base.BeforeClearCollectionChanged -= funcDelegateToRemove;
                }

                setFuncs.Clear();
                return true;
            }
        }

        /// <summary>
        /// Use in place of CollectionChanged Subtract
        /// </summary>
        /// <param name="objTag">Tag of delegate to remove from CollectionChanged</param>
        /// <param name="token">CancellationToken to listen to.</param>
        /// <returns>True if a delegate associated with the tag was found and deleted, false otherwise.</returns>
        public async Task<bool> RemoveTaggedBeforeClearCollectionChangedAsync(object objTag, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!_dicTaggedAddedBeforeClearDelegates.TryGetValue(
                        objTag, out HashSet<NotifyCollectionChangedEventHandler> setFuncs))
                {
                    Utils.BreakIfDebug();
                    return false;
                }

                foreach (NotifyCollectionChangedEventHandler funcDelegateToRemove in setFuncs)
                {
                    base.BeforeClearCollectionChanged -= funcDelegateToRemove;
                }

                setFuncs.Clear();
                return true;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Use in place of CollectionChangedAsync Adder
        /// </summary>
        /// <param name="objTag">Tag to associate with added delegate</param>
        /// <param name="funcDelegateToAdd">Delegate to add to CollectionChanged</param>
        /// <returns>True if delegate was successfully added, false if a delegate already exists with the associated tag.</returns>
        public bool AddTaggedBeforeClearCollectionChanged(object objTag, AsyncNotifyCollectionChangedEventHandler funcDelegateToAdd)
        {
            HashSet<AsyncNotifyCollectionChangedEventHandler> setFuncs
                = _dicTaggedAddedAsyncBeforeClearDelegates.GetOrAdd(
                    objTag, x => new HashSet<AsyncNotifyCollectionChangedEventHandler>());
            if (setFuncs.Add(funcDelegateToAdd))
            {
                base.CollectionChangedAsync += funcDelegateToAdd;
                return true;
            }
            Utils.BreakIfDebug();
            return false;
        }

        /// <summary>
        /// Use in place of CollectionChangedAsync Adder
        /// </summary>
        /// <param name="objTag">Tag to associate with added delegate</param>
        /// <param name="funcDelegateToAdd">Delegate to add to CollectionChanged</param>
        /// <param name="token">CancellationToken to listen to.</param>
        /// <returns>True if delegate was successfully added, false if a delegate already exists with the associated tag.</returns>
        public async Task<bool> AddTaggedBeforeClearCollectionChangedAsync(object objTag, AsyncNotifyCollectionChangedEventHandler funcDelegateToAdd, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                HashSet<AsyncNotifyCollectionChangedEventHandler> setFuncs
                    = _dicTaggedAddedAsyncBeforeClearDelegates.GetOrAdd(
                        objTag, x => new HashSet<AsyncNotifyCollectionChangedEventHandler>());

                if (setFuncs.Add(funcDelegateToAdd))
                {
                    base.BeforeClearCollectionChangedAsync += funcDelegateToAdd;
                    return true;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            Utils.BreakIfDebug();
            return false;
        }

        /// <summary>
        /// Use in place of CollectionChangedAsync Subtract
        /// </summary>
        /// <param name="objTag">Tag of delegate to remove from CollectionChanged</param>
        /// <param name="token">CancellationToken to listen to.</param>
        /// <returns>True if a delegate associated with the tag was found and deleted, false otherwise.</returns>
        public bool RemoveTaggedAsyncBeforeClearCollectionChanged(object objTag, CancellationToken token = default)
        {
            using (LockObject.EnterWriteLock(token))
            {
                if (!_dicTaggedAddedAsyncBeforeClearDelegates.TryGetValue(
                        objTag, out HashSet<AsyncNotifyCollectionChangedEventHandler> setFuncs))
                {
                    Utils.BreakIfDebug();
                    return false;
                }

                foreach (AsyncNotifyCollectionChangedEventHandler funcDelegateToRemove in setFuncs)
                {
                    base.BeforeClearCollectionChangedAsync -= funcDelegateToRemove;
                }

                setFuncs.Clear();
                return true;
            }
        }

        /// <summary>
        /// Use in place of CollectionChangedAsync Subtract
        /// </summary>
        /// <param name="objTag">Tag of delegate to remove from CollectionChanged</param>
        /// <param name="token">CancellationToken to listen to.</param>
        /// <returns>True if a delegate associated with the tag was found and deleted, false otherwise.</returns>
        public async Task<bool> RemoveTaggedAsyncBeforeClearCollectionChangedAsync(object objTag, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!_dicTaggedAddedAsyncBeforeClearDelegates.TryGetValue(
                        objTag, out HashSet<AsyncNotifyCollectionChangedEventHandler> setFuncs))
                {
                    Utils.BreakIfDebug();
                    return false;
                }

                foreach (AsyncNotifyCollectionChangedEventHandler funcDelegateToRemove in setFuncs)
                {
                    base.BeforeClearCollectionChangedAsync -= funcDelegateToRemove;
                }

                setFuncs.Clear();
                return true;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public override event NotifyCollectionChangedEventHandler BeforeClearCollectionChanged
        {
            add => throw new NotSupportedException("TaggedObservableCollection should use AddTaggedBeforeClearCollectionChanged method instead of adding to BeforeClearCollectionChanged");
            remove => throw new NotSupportedException("TaggedObservableCollection should use RemoveTaggedBeforeClearCollectionChanged method instead of removing from BeforeClearCollectionChanged");
        }

        /// <inheritdoc />
        public override event AsyncNotifyCollectionChangedEventHandler BeforeClearCollectionChangedAsync
        {
            add => throw new NotSupportedException("TaggedObservableCollection should use AddTaggedBeforeClearCollectionChanged method instead of adding to BeforeClearCollectionChangedAsync");
            remove => throw new NotSupportedException("TaggedObservableCollection should use RemoveTaggedAsyncBeforeClearCollectionChanged method instead of removing from BeforeClearCollectionChangedAsync");
        }
    }
}
