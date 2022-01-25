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

namespace Chummer
{
    public class MostRecentlyUsedCollection<T> : ThreadSafeObservableCollectionWithMaxSize<T>
    {
        public MostRecentlyUsedCollection(int intMaxSize) : base(intMaxSize)
        {
        }

        public MostRecentlyUsedCollection(List<T> list, int intMaxSize) : base(list, intMaxSize)
        {
        }

        public MostRecentlyUsedCollection(IEnumerable<T> collection, int intMaxSize) : base(collection, intMaxSize)
        {
        }

        private bool _blnSkipCollectionChanged;

        /// <inheritdoc />
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            using (new EnterUpgradeableReadLock(LockerObject))
            {
                if (_blnSkipCollectionChanged)
                    return;
                if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    _blnSkipCollectionChanged = true;
                    using (new EnterWriteLock(LockerObject))
                    {
                        // Remove all duplicate entries
                        for (int intLastIndex = Count - 1; intLastIndex >= 0; --intLastIndex)
                        {
                            T objItem = this[intLastIndex];
                            for (int intIndex = IndexOf(objItem); intIndex != intLastIndex; intIndex = IndexOf(objItem))
                            {
                                RemoveAt(intIndex);
                                --intLastIndex;
                            }
                        }
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
            using (new EnterWriteLock(LockerObject))
            {
                int intExistingIndex = IndexOf(item);
                if (intExistingIndex == -1)
                    base.InsertItem(index, item);
                else
                    MoveItem(intExistingIndex, Math.Min(index, Count - 1));
            }
        }
    }
}
