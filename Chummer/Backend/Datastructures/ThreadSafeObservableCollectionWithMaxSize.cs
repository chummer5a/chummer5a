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

using System.Collections.Generic;
using System.Collections.Specialized;

namespace Chummer
{
    public class ThreadSafeObservableCollectionWithMaxSize<T> : ThreadSafeObservableCollection<T>
    {
        private readonly int _intMaxSize;

        public ThreadSafeObservableCollectionWithMaxSize(int intMaxSize)
        {
            _intMaxSize = intMaxSize;
        }

        public ThreadSafeObservableCollectionWithMaxSize(List<T> list, int intMaxSize) : base(list)
        {
            _intMaxSize = intMaxSize;
            _blnSkipCollectionChanged = true;
            using (new EnterWriteLock(LockObject))
            {
                while (Count > _intMaxSize)
                    RemoveAt(Count - 1);
            }
            _blnSkipCollectionChanged = false;
        }

        public ThreadSafeObservableCollectionWithMaxSize(IEnumerable<T> collection, int intMaxSize) : base(collection)
        {
            _intMaxSize = intMaxSize;
            _blnSkipCollectionChanged = true;
            using (new EnterWriteLock(LockObject))
            {
                while (Count > _intMaxSize)
                    RemoveAt(Count - 1);
            }
            _blnSkipCollectionChanged = false;
        }

        private bool _blnSkipCollectionChanged;

        /// <inheritdoc />
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            using (new EnterUpgradeableReadLock(LockObject))
            {
                if (_blnSkipCollectionChanged)
                    return;
                if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    _blnSkipCollectionChanged = true;
                    using (new EnterWriteLock(LockObject))
                    {
                        // Remove all entries greater than the allowed size
                        while (Count > _intMaxSize)
                            RemoveItem(Count - 1);
                    }
                    _blnSkipCollectionChanged = false;
                }
            }
            base.OnCollectionChanged(e);
        }

        /// <inheritdoc />
        protected override void InsertItem(int index, T item)
        {
            // Immediately enter a write lock to prevent attempted reads until we have either inserted the item we want to insert or failed to do so
            using (new EnterWriteLock(LockObject))
            {
                if (index >= _intMaxSize)
                    return;
                while (Count >= _intMaxSize)
                    RemoveItem(Count - 1);
                base.InsertItem(index, item);
            }
        }

        /// <inheritdoc />
        public override bool TryAdd(T item)
        {
            // Immediately enter a write lock to prevent attempted reads until we have either added the item we want to add or failed to do so
            using (new EnterWriteLock(LockObject))
            {
                if (Count >= _intMaxSize)
                    return false;
                Add(item);
                return true;
            }
        }
    }
}
