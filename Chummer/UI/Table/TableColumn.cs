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
    public class TableColumn<T> : IDisposable where T : INotifyPropertyChanged
    {
        private readonly Func<TableCell> _cellFactory;
        private Comparison<object> _sorter;
        private bool _blnLive;
        private string _strText;
        private string _strTag;
        private int _intMinWidth;
        private int _intPrefWidth;
        private Func<T, object> _funcExtractor;
        private Comparison<T> _itemSorter;
        private readonly HashSet<string> _setDependencies = Utils.StringHashSetPool.Get();

        public TableColumn(Func<TableCell> cellFactory)
        {
            _cellFactory = cellFactory ?? throw new ArgumentNullException(nameof(cellFactory));
        }

        /// <summary>
        /// Add an additional dependency to the dependencies
        /// of this column.
        /// </summary>
        /// <param name="strDependency">the dependency to add</param>
        public void AddDependency(string strDependency)
        {
            CheckLive();
            _setDependencies.Add(strDependency);
        }

        /// <summary>
        /// Throw an InvalidOperationException in case the column is in
        /// live state.
        /// </summary>
        protected void CheckLive(bool blnExpectedValue = false)
        {
            if (_blnLive != blnExpectedValue)
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
                if (_funcExtractor == null)
                {
                    _itemSorter = (i1, i2) => _sorter(i1, i2);
                }
                else
                {
                    _itemSorter = (i1, i2) => _sorter(_funcExtractor(i1), _funcExtractor(i2));
                }
            }
            return _itemSorter;
        }

        #region Properties

        /// <summary>
        /// The dependencies as enumerable.
        /// </summary>
        internal IEnumerable<string> Dependencies => _setDependencies;

        /// <summary>
        /// Method for extracting the value for the cell from
        /// the item
        /// </summary>
        public Func<T, object> Extractor
        {
            get => _funcExtractor;
            set
            {
                CheckLive();
                _funcExtractor = value;
            }
        }

        /// <summary>
        /// Property indicating whether this column is live or not.
        /// </summary>
        protected bool Live => _blnLive;

        /// <summary>
        /// The minimal width for this column.
        /// </summary>
        public int MinWidth
        {
            get => _intMinWidth;
            set
            {
                CheckLive();
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(MinWidth));
                }
                _intMinWidth = value;
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
            get => _strText;
            set
            {
                CheckLive();
                _strText = value;
            }
        }

        /// <summary>
        /// The tag for the header.
        /// </summary>
        public string Tag
        {
            get => _strTag;
            set
            {
                CheckLive();
                _strTag = value;
            }
        }

        /// <summary>
        /// the preferred width of the column
        /// </summary>
        public int PrefWidth
        {
            get => _intPrefWidth;
            set
            {
                if (value >= _intMinWidth)
                {
                    _intPrefWidth = value;
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
            _blnLive = true;
        }

        #endregion Properties

        /// <inheritdoc />
        public void Dispose()
        {
            Utils.StringHashSetPool.Return(_setDependencies);
        }
    }
}
