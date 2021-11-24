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
using System.ComponentModel;

namespace Chummer
{
    public class CachedBindingList<T> : BindingList<T>
    {
        public event EventHandler<RemovingOldEventArgs> BeforeRemove;

        protected override void RemoveItem(int index)
        {
            BeforeRemove?.Invoke(this, new RemovingOldEventArgs(Items[index], index));

            base.RemoveItem(index);
        }

        protected override void ClearItems()
        {
            for (int i = 0; i < Items.Count; ++i)
            {
                BeforeRemove?.Invoke(this, new RemovingOldEventArgs(Items[i], i));
            }

            base.ClearItems();
        }

        protected override void SetItem(int index, T item)
        {
            T objOldItem = Items[index];
            if (!objOldItem.Equals(item))
            {
                BeforeRemove?.Invoke(this, new RemovingOldEventArgs(objOldItem, index));
            }
            base.SetItem(index, item);
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
}
