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
    /// <inheritdoc />
    /// <summary>
    /// ObservableCollection that allows for adding and removal of anonymous delegates by an associated tag
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TaggedObservableCollection<T> : ObservableCollection<T>
    {
        private readonly Dictionary<object, NotifyCollectionChangedEventHandler> _dicTaggedAddedDelegates = new Dictionary<object, NotifyCollectionChangedEventHandler>();

        /// <summary>
        /// Use in place of CollectionChanged Adder
        /// </summary>
        /// <param name="objTag">Tag to associate with added delegate</param>
        /// <param name="funcDelegateToAdd">Delegate to add to CollectionChanged</param>
        /// <returns>True if delegate was successfully added, false if a delegate already exists with the associated tag.</returns>
        public bool AddTaggedCollectionChanged(object objTag, NotifyCollectionChangedEventHandler funcDelegateToAdd)
        {
            if (!_dicTaggedAddedDelegates.ContainsKey(objTag))
            {
                _dicTaggedAddedDelegates.Add(objTag, funcDelegateToAdd);
                CollectionChanged += funcDelegateToAdd;
                return true;
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
            if (_dicTaggedAddedDelegates.TryGetValue(objTag, out NotifyCollectionChangedEventHandler funcDelegateToRemove))
            {
                CollectionChanged -= funcDelegateToRemove;
                _dicTaggedAddedDelegates.Remove(objTag);
                return true;
            }
            Utils.BreakIfDebug();
            return false;
        }
    }
}
