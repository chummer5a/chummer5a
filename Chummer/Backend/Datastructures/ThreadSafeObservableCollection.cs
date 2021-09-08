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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

namespace Chummer
{
    public class ThreadSafeObservableCollection<T> : ObservableCollection<T>, IDisposable
    {
        private readonly ReaderWriterLockSlim
            _rwlThis = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public ThreadSafeObservableCollection()
        {
        }

        public ThreadSafeObservableCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        public ThreadSafeObservableCollection(List<T> list) : base(list)
        {
        }

        public new int Count
        {
            get
            {
                using (new EnterReadLock(_rwlThis))
                    return base.Count;
            }
        }

        public new T this[int index]
        {
            get
            {
                using (new EnterReadLock(_rwlThis))
                    return base[index];
            }
            set
            {
                using (new EnterUpgradeableReadLock(_rwlThis))
                {
                    if (base[index].Equals(value))
                        return;
                    using (new EnterWriteLock(_rwlThis))
                        base[index] = value;
                }
            }
        }

        public new bool Contains(T item)
        {
            using (new EnterReadLock(_rwlThis))
                return base.Contains(item);
        }

        public new void CopyTo(T[] array, int arrayIndex)
        {
            using (new EnterReadLock(_rwlThis))
                base.CopyTo(array, arrayIndex);
        }

        public new IEnumerator<T> GetEnumerator()
        {
            using (new EnterReadLock(_rwlThis))
                return base.GetEnumerator();
        }

        public new int IndexOf(T item)
        {
            using (new EnterReadLock(_rwlThis))
                return base.IndexOf(item);
        }

        public override event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                using (new EnterWriteLock(_rwlThis))
                    base.CollectionChanged += value;
            }
            remove
            {
                using (new EnterWriteLock(_rwlThis))
                    base.CollectionChanged -= value;
            }
        }

        protected override event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                using (new EnterWriteLock(_rwlThis))
                    base.PropertyChanged += value;
            }
            remove
            {
                using (new EnterWriteLock(_rwlThis))
                    base.PropertyChanged -= value;
            }
        }

        protected override void InsertItem(int index, T item)
        {
            using (new EnterWriteLock(_rwlThis))
                base.InsertItem(index, item);
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            using (new EnterWriteLock(_rwlThis))
                base.MoveItem(oldIndex, newIndex);
        }

        protected override void ClearItems()
        {
            using (new EnterWriteLock(_rwlThis))
                base.ClearItems();
        }

        protected override void RemoveItem(int index)
        {
            using (new EnterWriteLock(_rwlThis))
                base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            using (new EnterWriteLock(_rwlThis))
                base.SetItem(index, item);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            using (new EnterUpgradeableReadLock(_rwlThis))
                base.OnPropertyChanged(e);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            using (new EnterUpgradeableReadLock(_rwlThis))
                base.OnCollectionChanged(e);
        }

        public void Dispose()
        {
            _rwlThis.Dispose();
        }
    }
}
