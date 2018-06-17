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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Chummer.UI.Table
{
    public sealed class TableColumnCollection<T> : IEnumerable<TableColumn<T>>
        where T : class, INotifyPropertyChanged
    {
        private readonly List<TableColumn<T>> _columns = new List<TableColumn<T>>();
        private readonly TableView<T> _table;

        internal TableColumnCollection(TableView<T> table)
        {
            _table = table;
        }

        /// <summary>
        /// Access column by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public TableColumn<T> this[int index] {
            get => _columns[index];
            //set => _columns[index] = value;
        }

        public int Count => _columns.Count;

        public void Add(TableColumn<T> column) {
            if (column == null)
            {
                throw new ArgumentException();
            }

            _columns.Add(column);
            _table.ColumnAdded(column);
        }

        public IEnumerator<TableColumn<T>> GetEnumerator()
        {
            return _columns.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _columns.GetEnumerator();
        }
    }
}
