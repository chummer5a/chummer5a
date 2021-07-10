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

namespace Chummer
{
    public class ObservableCollectionWithMaxSize<T> : ObservableCollection<T>
    {
        private readonly int _intMaxSize;

        public ObservableCollectionWithMaxSize(int intMaxSize)
        {
            _intMaxSize = intMaxSize;
        }

        public ObservableCollectionWithMaxSize(List<T> list, int intMaxSize) : base(list)
        {
            _intMaxSize = intMaxSize;
            while (Count > _intMaxSize)
                RemoveAt(Count - 1);
        }

        public ObservableCollectionWithMaxSize(IEnumerable<T> collection, int intMaxSize) : base(collection)
        {
            _intMaxSize = intMaxSize;
            while (Count > _intMaxSize)
                RemoveAt(Count - 1);
        }

        public new virtual void Add(T item)
        {
            if (Count <= _intMaxSize)
                base.Add(item);
        }

        protected override void InsertItem(int index, T item)
        {
            if (index < _intMaxSize)
            {
                base.InsertItem(index, item);
                while (Count > _intMaxSize)
                    RemoveAt(Count - 1);
            }
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            if (newIndex >= _intMaxSize)
                newIndex = _intMaxSize;
            base.MoveItem(oldIndex, newIndex);
        }
    }
}