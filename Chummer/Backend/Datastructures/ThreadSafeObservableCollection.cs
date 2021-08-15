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
using System.Collections.ObjectModel;

namespace Chummer
{
    public class ThreadSafeObservableCollection<T> : ObservableCollection<T>
    {
        private readonly object _objLock = new object();

        public new T this[int index]
        {
            get
            {
                lock (_objLock)
                    return base[index];
            }
            set
            {
                lock (_objLock)
                    base[index] = value;
            }
        }

        public new int Count
        {
            get
            {
                lock (_objLock)
                    return base.Count;
            }
        }

        protected new IList<T> Items
        {
            get
            {
                lock (_objLock)
                    return base.Items;
            }
        }

        public new virtual void Add(T item)
        {
            lock (_objLock)
                base.Add(item);
        }

        public new virtual bool Contains(T item)
        {
            lock (_objLock)
                return base.Contains(item);
        }

        public new void CopyTo(T[] array, int index)
        {
            lock (_objLock)
                base.CopyTo(array, index);
        }

        public new IEnumerator<T> GetEnumerator()
        {
            lock (_objLock)
                return base.GetEnumerator();
        }

        public new int IndexOf(T item)
        {
            lock (_objLock)
                return base.IndexOf(item);
        }

        protected override void InsertItem(int index, T item)
        {
            lock (_objLock)
                base.InsertItem(index, item);
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            lock (_objLock)
                base.MoveItem(oldIndex, newIndex);
        }

        public new void Move(int oldIndex, int newIndex)
        {
            lock (_objLock)
                base.Move(oldIndex, newIndex);
        }

        protected new IDisposable BlockReentrancy()
        {
            lock (_objLock)
                return base.BlockReentrancy();
        }

        protected new void CheckReentrancy()
        {
            lock (_objLock)
                base.CheckReentrancy();
        }

        protected override void ClearItems()
        {
            lock (_objLock)
                base.ClearItems();
        }

        protected override void RemoveItem(int index)
        {
            lock (_objLock)
                base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            lock (_objLock)
                base.SetItem(index, item);
        }
    }
}
