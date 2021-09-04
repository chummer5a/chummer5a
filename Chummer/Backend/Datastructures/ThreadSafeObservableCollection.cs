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

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Chummer
{
    public class ThreadSafeObservableCollection<T> : ObservableCollection<T>
    {
        private readonly object _objLock = new object();

        public override string ToString()
        {
            lock (_objLock)
                return base.ToString();
        }

        public override int GetHashCode()
        {
            lock (_objLock)
                // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
                return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            lock (_objLock)
                // ReSharper disable once BaseObjectEqualsIsObjectEquals
                return base.Equals(obj);
        }

        public override event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                lock (_objLock)
                    base.CollectionChanged += value;
            }
            remove
            {
                lock (_objLock)
                    base.CollectionChanged -= value;
            }
        }

        protected override event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                lock (_objLock)
                    base.PropertyChanged += value;
            }
            remove
            {
                lock (_objLock)
                    base.PropertyChanged -= value;
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            lock (_objLock)
                base.OnPropertyChanged(e);
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

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            lock (_objLock)
                base.OnCollectionChanged(e);
        }
    }
}
