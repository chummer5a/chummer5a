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
using System.Collections.ObjectModel;
using System.Collections.Specialized;

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
        /// To make things easy, all of the collections elements will be present in e.NewItems.
        /// </summary>
        public event NotifyCollectionChangedEventHandler BeforeClearCollectionChanged;

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
            using (BlockReentrancy())
            {
                BeforeClearCollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, Items));
            }
            base.ClearItems();
        }
    }
}
