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
using System.ComponentModel;

namespace Chummer.UI.Table
{
    /// <summary>
    /// Class containing information on how to visually represent
    /// a item in a certain table column.
    /// </summary>
    /// <typeparam name="T">the table item type</typeparam>
    public class TableColumn<T>
        where T : INotifyPropertyChanged
    {

        private readonly Func<TableCell> _cellFactory;
        private Comparison<object> _sorter;
        private bool _live = false;
        private string _text;
        private string _tag;
        private int _minWidth = 0;
        private int _prefWidth = 0;
        private Func<T, object> _extractor;
        private Comparison<T> _itemSorter;
        private readonly HashSet<string> _dependencies = new HashSet<string>();

        public TableColumn(Func<TableCell> cellFactory)
        {
            _cellFactory = cellFactory ?? throw new ArgumentNullException(nameof(cellFactory));
        }

        public static TableColumn<U> CreateColumn<U, V>()
            where U : INotifyPropertyChanged
            where V : TableCell, new()
        {
            TableColumn<U> column = new TableColumn<U>(() => new V());
            return column;
        }

        /// <summary>
        /// Add an additional dependency to the dependencies
        /// of this column.
        /// </summary>
        /// <param name="dependency">the dependency to add</param>
        public void AddDependency(string dependency)
        {
            CheckLive();
            _dependencies.Add(dependency);
        }

        /// <summary>
        /// Throw an InvalidOperationException in case the column is in
        /// live state.
        /// </summary>
        protected void CheckLive(bool expectedValue = false)
        {
            if (_live != expectedValue)
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// create a cell instance
        /// </summary>
        /// <returns>a new instance of the cell</returns>
        internal TableCell CreateCell() => _cellFactory();

        internal Comparison<T> CreateSorter()
        {
            if (_itemSorter == null && _sorter != null)
            {
                if (_extractor == null)
                {
                    _itemSorter = (i1, i2) => _sorter(i1, i2);
                }
                else
                {
                    _itemSorter = (i1, i2) => _sorter(_extractor(i1), _extractor(i2));
                }
            }
            return _itemSorter;
        }

        #region Properties
        /// <summary>
        /// The dependencies as enumerable.
        /// </summary>
        internal IEnumerable<string> Dependencies => _dependencies;

        /// <summary>
        /// Method for extracting the value for the cell from
        /// the item
        /// </summary>
        public Func<T, object> Extractor
        {
            get => _extractor;
            set {
                CheckLive();
                _extractor = value;
            }
        }

        /// <summary>
        /// Property indicating whether this column is live or not.
        /// </summary>
        protected bool Live => _live;

        /// <summary>
        /// The minimal width for this column.
        /// </summary>
        public int MinWidth
        {
            get => _minWidth;
            set {
                CheckLive();
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _minWidth = value;
            }
        }

        /// <summary>
        /// sorter for this column
        /// </summary>
        public Comparison<object> Sorter
        {
            get => _sorter;
            set
            {
                CheckLive();
                _sorter = value;
            }
        }

        /// <summary>
        /// The text that should be displayed in the header.
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                CheckLive();
                _text = value;
            }
        }

        /// <summary>
        /// The tag for the header.
        /// </summary>
        public string Tag
        {
            get => _tag;
            set
            {
                CheckLive();
                _tag = value;
            }
        }

        /// <summary>
        /// the preferred width of the column
        /// </summary>
        public int PrefWidth
        {
            get => _prefWidth;
            set {
                if (value >= _minWidth)
                {
                    _prefWidth = value;
                }
            }
        }

        /// <summary>
        /// Extractor for tooltip text on cell
        /// </summary>
        public Func<T, string> ToolTipExtractor { get; set; }

        /// <summary>
        /// transfer the column to the live state
        /// </summary>
        internal void MakeLive()
        {
            CheckLive();
            _live = true;
        }
        #endregion
    }
}
