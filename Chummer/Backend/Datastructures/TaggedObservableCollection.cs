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
using System.Collections.Specialized;
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
        private readonly LockingDictionary<object, HashSet<NotifyCollectionChangedEventHandler>> _dicTaggedAddedDelegates = new LockingDictionary<object, HashSet<NotifyCollectionChangedEventHandler>>();
        private readonly LockingDictionary<object, HashSet<NotifyCollectionChangedEventHandler>> _dicTaggedAddedBeforeClearDelegates = new LockingDictionary<object, HashSet<NotifyCollectionChangedEventHandler>>();

        /// <summary>
        /// Use in place of CollectionChanged Adder
        /// </summary>
        /// <param name="objTag">Tag to associate with added delegate</param>
        /// <param name="funcDelegateToAdd">Delegate to add to CollectionChanged</param>
        /// <returns>True if delegate was successfully added, false if a delegate already exists with the associated tag.</returns>
        public bool AddTaggedCollectionChanged(object objTag, NotifyCollectionChangedEventHandler funcDelegateToAdd)
        {
            if (!_dicTaggedAddedDelegates.TryGetValue(objTag, out HashSet<NotifyCollectionChangedEventHandler> setFuncs))
            {
                setFuncs = new HashSet<NotifyCollectionChangedEventHandler>();
                _dicTaggedAddedDelegates.Add(objTag, setFuncs);
            }
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
        /// <returns>True if delegate was successfully added, false if a delegate already exists with the associated tag.</returns>
        public async ValueTask<bool> AddTaggedCollectionChangedAsync(object objTag, NotifyCollectionChangedEventHandler funcDelegateToAdd)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                (bool blnSuccess, HashSet<NotifyCollectionChangedEventHandler> setFuncs)
                    = await _dicTaggedAddedDelegates.TryGetValueAsync(objTag);
                if (!blnSuccess)
                {
                    setFuncs = new HashSet<NotifyCollectionChangedEventHandler>();
                    await _dicTaggedAddedDelegates.AddAsync(objTag, setFuncs);
                }

                if (setFuncs.Add(funcDelegateToAdd))
                {
                    base.CollectionChanged += funcDelegateToAdd;
                    return true;
                }
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
            Utils.BreakIfDebug();
            return false;
        }

        /// <summary>
        /// Use in place of CollectionChanged Subtract
        /// </summary>
        /// <param name="objTag">Tag of delegate to remove from CollectionChanged</param>
        /// <returns>True if a delegate associated with the tag was found and deleted, false otherwise.</returns>
        public bool RemoveTaggedCollectionChanged(object objTag)
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

        /// <summary>
        /// Use in place of CollectionChanged Subtract
        /// </summary>
        /// <param name="objTag">Tag of delegate to remove from CollectionChanged</param>
        /// <returns>True if a delegate associated with the tag was found and deleted, false otherwise.</returns>
        public async ValueTask<bool> RemoveTaggedCollectionChangedAsync(object objTag)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                (bool blnSuccess, HashSet<NotifyCollectionChangedEventHandler> setFuncs)
                    = await _dicTaggedAddedDelegates.TryGetValueAsync(objTag);
                if (!blnSuccess)
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
                await objLocker.DisposeAsync();
            }
        }

        /// <inheritdoc />
        public override event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => throw new NotSupportedException("TaggedObservableCollection should use AddTaggedCollectionChanged method instead of adding to CollectionChanged");
            remove => throw new NotSupportedException("TaggedObservableCollection should use RemoveTaggedCollectionChanged method instead of removing from CollectionChanged");
        }

        /// <summary>
        /// Use in place of CollectionChanged Adder
        /// </summary>
        /// <param name="objTag">Tag to associate with added delegate</param>
        /// <param name="funcDelegateToAdd">Delegate to add to CollectionChanged</param>
        /// <returns>True if delegate was successfully added, false if a delegate already exists with the associated tag.</returns>
        public bool AddTaggedBeforeClearCollectionChanged(object objTag, NotifyCollectionChangedEventHandler funcDelegateToAdd)
        {
            if (!_dicTaggedAddedBeforeClearDelegates.TryGetValue(objTag, out HashSet<NotifyCollectionChangedEventHandler> setFuncs))
            {
                setFuncs = new HashSet<NotifyCollectionChangedEventHandler>();
                _dicTaggedAddedBeforeClearDelegates.Add(objTag, setFuncs);
            }
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
        /// <returns>True if delegate was successfully added, false if a delegate already exists with the associated tag.</returns>
        public async ValueTask<bool> AddTaggedBeforeClearCollectionChangedAsync(object objTag, NotifyCollectionChangedEventHandler funcDelegateToAdd)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                (bool blnSuccess, HashSet<NotifyCollectionChangedEventHandler> setFuncs)
                    = await _dicTaggedAddedBeforeClearDelegates.TryGetValueAsync(objTag);
                if (!blnSuccess)
                {
                    setFuncs = new HashSet<NotifyCollectionChangedEventHandler>();
                    await _dicTaggedAddedBeforeClearDelegates.AddAsync(objTag, setFuncs);
                }

                if (setFuncs.Add(funcDelegateToAdd))
                {
                    base.BeforeClearCollectionChanged += funcDelegateToAdd;
                    return true;
                }
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
            Utils.BreakIfDebug();
            return false;
        }

        /// <summary>
        /// Use in place of CollectionChanged Subtract
        /// </summary>
        /// <param name="objTag">Tag of delegate to remove from CollectionChanged</param>
        /// <returns>True if a delegate associated with the tag was found and deleted, false otherwise.</returns>
        public bool RemoveTaggedBeforeClearCollectionChanged(object objTag)
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

        /// <summary>
        /// Use in place of CollectionChanged Subtract
        /// </summary>
        /// <param name="objTag">Tag of delegate to remove from CollectionChanged</param>
        /// <returns>True if a delegate associated with the tag was found and deleted, false otherwise.</returns>
        public async ValueTask<bool> RemoveTaggedBeforeClearCollectionChangedAsync(object objTag)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                (bool blnSuccess, HashSet<NotifyCollectionChangedEventHandler> setFuncs)
                    = await _dicTaggedAddedBeforeClearDelegates.TryGetValueAsync(objTag);
                if (!blnSuccess)
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
                await objLocker.DisposeAsync();
            }
        }

        /// <inheritdoc />
        public override event NotifyCollectionChangedEventHandler BeforeClearCollectionChanged
        {
            add => throw new NotSupportedException("TaggedObservableCollection should use AddTaggedBeforeClearCollectionChanged method instead of adding to BeforeClearCollectionChanged");
            remove => throw new NotSupportedException("TaggedObservableCollection should use RemoveTaggedBeforeClearCollectionChanged method instead of removing from BeforeClearCollectionChanged");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dicTaggedAddedDelegates.Dispose();
                _dicTaggedAddedBeforeClearDelegates.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
