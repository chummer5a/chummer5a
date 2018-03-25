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
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace Chummer.UI.Table
{
    public partial class TableView<T> : UserControl where T : class, INotifyPropertyChanged
    {

        protected class TableLayoutEngine : LayoutEngine
        {

            private readonly TableView<T> _table;

            public TableLayoutEngine(TableView<T> table)
            {
                _table = table;
            }

            public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
            {
                _table.SuspendLayout();
                int[] widths = new int[_table._columns.Count];

                int y = 0;

                // determine minimal widths
                for (int i = 0; i < widths.Length; i++)
                {
                    Size headerMinSize = _table._cells[i].header.MinimumSize;
                    int width = Math.Max(_table._columns[i].MinWidth, headerMinSize.Width);
                    if (y < headerMinSize.Height)
                    {
                        y = headerMinSize.Height;
                    }
                    widths[i] = width;
                }

                for (int i = 0; i < _table._rowCells.Count; i++)
                {
                    TableRow row = _table._rowCells[i];
                    if (row.Parent != null)
                    {
                        for (int j = 0; j < widths.Length; j++)
                        {
                            int w = _table._cells[j].cells[i].MinimumSize.Width;
                            if (widths[j] < w)
                            {
                                widths[j] = w;
                            }
                        }
                    }
                }

                int widthSum = 0;
                foreach (int w in widths)
                {
                    widthSum += w;
                }

                if (widthSum > _table.Width)
                {
                    // modify container size if content does not fit
                    _table.Width = widthSum;
                }
                else
                {
                    // grow columns to preferred sizes starting at the leftmost
                    int delta = _table.Width - widthSum;
                    for (int i = 0; delta > 0 && i < widths.Length; i++)
                    {
                        int w = widths[i];
                        int pW = _table._columns[i].PrefWidth;
                        if (w < pW)
                        {
                            int diff = pW - w;
                            if (delta > diff)
                            {
                                widths[i] = pW;
                                delta -= diff;
                            }
                            else
                            {
                                widths[i] = w + delta;
                                break;
                            }
                        }
                    }
                }

                int x = 0;

                // layout headers
                for (int i = 0; i < widths.Length; i++)
                {
                    Control header = _table._cells[i].header;
                    header.Location = new System.Drawing.Point(x, 0);
                    header.Size = new Size(widths[i], y);
                    x += widths[i];
                }

                int rowIndex = 0;

                // layout cells
                foreach (int index in _table._permutation)
                {
                    int dy = 0;
                    x = 0;
                    TableRow row = _table._rowCells[index];
                    if (row.Parent != null)
                    {
                        row.SuspendLayout();
                        row.Index = rowIndex;
                        row.Selected = (_table._selectedIndex == index);
                        row.Location = new Point(0, y);
                        row.Width = widthSum;
                        for (int i = 0; i < _table._columns.Count; i++)
                        {
                            TableCell cell = _table._cells[i].cells[index];
                            cell.Location = new System.Drawing.Point(x, row.Margin.Top);
                            if (dy < cell.Height)
                            {
                                dy = cell.Height;
                            }
                            x += widths[i];
                        }
                        for (int i = 0; i < _table._columns.Count; i++)
                        {
                            TableCell cell = _table._cells[i].cells[index];
                            cell.UpdateAvailableSize(widths[i], dy);
                        }
                        row.Height = dy + row.Margin.Top + row.Margin.Bottom;
                        row.ResumeLayout(false);
                        y += row.Height;
                        rowIndex++;
                    }
                }
                if (y > _table.Height)
                {
                    _table.Height = y;
                }
                _table.ResumeLayout(false);
                return true;
            }
        }

        private class ColumnHolder
        {
            public readonly HeaderCell header;
            public readonly List<TableCell> cells;

            public ColumnHolder(HeaderCell header, List<TableCell> cells)
            {
                this.header = header;
                this.cells = cells;
            }
        }

        private readonly Predicate<T> _defaultFilter;
        private TableColumn<T> _sortColumn;
        private BindingList<T> _items;
        private Predicate<T> _filter;
        private readonly TableColumnCollection<T> _columns;
        private TableLayoutEngine _layoutEngine;
        private readonly List<ColumnHolder> _cells = new List<ColumnHolder>();
        private readonly Dictionary<string, List<int>> _observedProperties = new Dictionary<string, List<int>>();
        private List<int> _permutation = new List<int>();
        private List<TableRow> _rowCells = new List<TableRow>();
        private SortOrder _sortType = SortOrder.None;
        private object _sortPausedSender = null;
        private int _selectedIndex = -1;

        public TableView()
        {
            _columns = new TableColumnCollection<T>(this);
            _defaultFilter = (item) => true;
            _filter = _defaultFilter;
            InitializeComponent();
        }

        private void ItemPropertyChanged(int index, T item, string property)
        {
            SuspendLayout();
            if (property == null || property == String.Empty)
            {
                // update all cells
                UpdateRow(index, item);
            }
            else
            {
                // update cells in columns that have the column as dependency
                List<int> columns;

                if (_observedProperties.TryGetValue(property, out columns))
                {
                    foreach (int columnIndex in columns)
                    {
                        UpdateCell(_columns[columnIndex], _cells[columnIndex].cells[index], item);
                    }
                }
            }
            ResumeLayout(true);
        }

        public void PauseSort(object sender)
        {
            _sortPausedSender = sender;
        }

        public void ResumeSort(object sender)
        {
            if (_sortPausedSender == sender)
            {
                _sortPausedSender = null;

                // prevent sort for focus loss when disposing
                if (_items != null && _permutation.Count == _items.Count) 
                {
                    Sort(true);
                }
            }
        }

        private void Sort(bool performLayout = true)
        {
            if (_sortPausedSender != null) return;
            if (_sortType == SortOrder.None || _sortColumn == null)
            {
                // sort by index
                _permutation.Sort();
            }
            else
            {
                Comparison<T> comparison = _sortColumn.CreateSorter();
                
                _permutation.Sort((i1, i2) => comparison(_items[i1], _items[i2]));
                if (_sortType == SortOrder.Descending)
                {
                    _permutation.Reverse();
                }
            }
            if (performLayout)
            {
                PerformLayout();
            }
        }

        private void UpdateCell(TableColumn<T> column, TableCell cell, T item)
        {
            Func<T, object> extractor = column.Extractor;
            cell.UpdateValue(extractor == null ? item : extractor(item));
            Func<T, string> tooltipExtractor = column.ToolTipExtractor;
            ToolTip tooltip = ToolTip;
            if (tooltip != null && tooltipExtractor != null)
            {
                Control content = cell.Content;
                string text = tooltipExtractor(item);
                tooltip.SetToolTip(content == null ? cell : content, text);
            }
        }

        private void UpdateRow(int index, T item)
        {
            for (int i = 0; i < _columns.Count; i++)
            {
                List<TableCell> cells = _cells[i].cells;
                TableColumn<T> column = _columns[i];
                TableCell cell = cells[index];
                UpdateCell(column, cell, item);
            }
        }

        private TableCell CreateCell(T item, TableColumn<T> column)
        {
            TableCell cell = column.CreateCell();
            UpdateCell(column, cell, item);
            return cell;
        }

        private void CreateCellsForColumn(int insertIndex, TableColumn<T> column) {
            SuspendLayout();
            List<TableCell> cells;
            if (_items != null)
            {
                cells = new List<TableCell>(_items.Count);
                for (int i = 0; i < _items.Count; i++)
                {
                    T item = _items[i];
                    TableCell cell = CreateCell(item, column);
                    cells.Add(cell);
                    if (_filter(item))
                    {
                        TableRow row = _rowCells[i];
                        row.SuspendLayout();
                        row.Controls.Add(cell);
                        row.ResumeLayout(false);
                    }
                }
            }
            else
            {
                cells = new List<TableCell>();
            }
            HeaderCell header = new HeaderCell()
            {
                Text = column.Text,
                TextTag = column.Tag
            };
            if (column.Sorter != null)
            {
                header.HeaderClick += (sender, evt) => {
                    if (_sortColumn == column)
                    {
                        // cycle through states if column remains the same
                        switch(_sortType)
                        {
                            case SortOrder.None:
                                _sortType = SortOrder.Ascending;
                                break;
                            case SortOrder.Ascending:
                                _sortType = SortOrder.Descending;
                                break;
                            case SortOrder.Descending:
                                _sortType = SortOrder.None;
                                break;
                        }
                    }
                    else
                    {
                        if (_sortColumn != null)
                        {
                            // reset old column sort arrow
                            for (int i = 0; i < _columns.Count; i++)
                            {
                                if (_columns[i] == _sortColumn)
                                {
                                    _cells[i].header.SortType = SortOrder.None;
                                    break;
                                }
                            }
                        }
                        _sortColumn = column;
                        _sortType = SortOrder.Ascending;
                    }
                    header.SortType = _sortType;
                    Sort();
                };
                header.Sortable = true;
            }
            Controls.Add(header);
            _cells.Insert(insertIndex, new ColumnHolder(header, cells));
            ResumeLayout(false);
        }

        internal void ColumnAdded(TableColumn<T> column) {
            column.MakeLive();
            int index = _columns.Count - 1;
            CreateCellsForColumn(index, column);
            foreach (string dependency in column.Dependencies)
            {
                if (!_observedProperties.TryGetValue(dependency, out List<int> list))
                {
                    list = new List<int>();
                    _observedProperties[dependency] = list;
                }
                list.Add(index);
            }
        }

        private void DisposeAll()
        {
            _permutation.Clear();
            foreach (ColumnHolder col in _cells)
            {
                foreach (TableCell cell in col.cells)
                {
                    cell.Dispose();
                }
            }
            Controls.Clear();
        }

        private void DoFilter(bool performLayout = true)
        {
            if (_items == null) return;
            SuspendLayout();

            for (int i = 0; i < _items.Count; i++)
            {
                TableRow row = _rowCells[i];
                row.SuspendLayout();
                if (_filter(_items[i]))
                {
                    if (row.Parent == null)
                    {
                        Controls.Add(row);
                    }
                }
                else
                {
                    if (row.Parent != null)
                    {
                        Controls.Remove(row);
                    }
                }
                row.ResumeLayout(false);
            }

            RestartLayout(performLayout);
        }

        private void RestartLayout(bool performLayout)
        {
            ResumeLayout(false);
            if (performLayout)
            {
                PerformLayout();
            }
        }

        /// <summary>
        /// Predicate for filtering the items.
        /// </summary>
        public Predicate<T> Filter
        {
            get => _filter;
            set
            {
                if (value != null || _filter != _defaultFilter)
                {
                    _filter = value ?? _defaultFilter;
                    DoFilter();
                }
            }
        }
 

        private void ItemsChanged(object sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemChanged:
                    TableRow row;
                    T item = _items[e.NewIndex];
                    if (e.PropertyDescriptor == null)
                    {
                        SuspendLayout();
                        row = _rowCells[e.NewIndex];
                        if (_filter(item))
                        {
                            if (row.Parent == null)
                            {
                                Controls.Add(row);
                            }
                        }
                        else if (row.Parent != null)
                        {
                            Controls.Remove(row);
                        }
                        UpdateRow(e.NewIndex, item);
                        Sort(false);
                        RestartLayout(true);
                    }
                    else
                    {
                        ItemPropertyChanged(e.NewIndex, item, e.PropertyDescriptor.Name);
                    }
                    break;
                case ListChangedType.ItemAdded:
                    item = _items[e.NewIndex];
                    row = CreateRow();
                    _rowCells.Insert(e.NewIndex, row);
                    SuspendLayout();
                    if (_filter(item))
                    {
                        Controls.Add(row);
                    }
                    row.SuspendLayout();
                    for (int i = 0; i < _columns.Count; i++)
                    {
                        TableColumn<T> column = _columns[i];
                        List<TableCell> cells = _cells[i].cells;
                        TableCell newCell = CreateCell(item, column);
                        cells.Insert(e.NewIndex, newCell);
                        row.Controls.Add(newCell);
                    }
                    row.ResumeLayout(false);
                    _permutation.Add(_permutation.Count);
                    Sort(false);
                    RestartLayout(true);
                    break;
                case ListChangedType.ItemDeleted:
                    SuspendLayout();
                    for (int i = 0; i < _columns.Count; i++)
                    {
                        TableColumn<T> column = _columns[i];
                        List<TableCell> cells = _cells[i].cells;
                        TableCell oldCell = cells[e.NewIndex];
                        cells.RemoveAt(e.NewIndex);
                    }
                    row = _rowCells[e.NewIndex];
                    if (row.Parent != null)
                    {
                        Controls.Remove(row);
                    }
                    row.Dispose();
                    _rowCells.RemoveAt(e.NewIndex);
                    _permutation.Remove(_permutation.Count - 1);
                    Sort(false);
                    RestartLayout(true);
                    break;
                case ListChangedType.ItemMoved:
                    foreach (ColumnHolder col in _cells)
                    {
                        List<TableCell> cells = col.cells;
                        TableCell cell = cells[e.OldIndex];
                        cells.RemoveAt(e.OldIndex);
                        cells.Insert(e.NewIndex, cell);
                    }
                    row = _rowCells[e.OldIndex];
                    _rowCells.RemoveAt(e.OldIndex);
                    _rowCells.Insert(e.NewIndex, row);

                    // fix permutation
                    int minIndex, maxIndex, delta;
                    if (e.OldIndex < e.NewIndex)
                    {
                        delta = -1;
                        minIndex = e.OldIndex + 1;
                        maxIndex = e.NewIndex;
                    }
                    else
                    {
                        delta = +1;
                        minIndex = e.NewIndex;
                        maxIndex = e.OldIndex - 1;
                    }
                    for (int i = 0; i < _permutation.Count; i++)
                    {
                        int value = _permutation[i];
                        if (value == e.OldIndex)
                        {
                            _permutation[i] = e.NewIndex;
                        }
                        else if (value >= minIndex && value <= maxIndex)
                        {
                            _permutation[i] = value + delta;
                        }
                    }
                    Sort(true);
                    break;
            }
        }

        /// <summary>
        /// The list of items displayed in the table.
        /// </summary>
        public BindingList<T> Items
        {
            get => _items;
            set
            {
                int oldCount = 0;
                if (_items != null)
                {
                    // remove listener from old items
                    _items.ListChanged -= ItemsChanged;
                    oldCount = _items.Count;
                    _permutation.Clear();
                }

                _items = value;
                int newCount = _items == null ? 0 : _items.Count;
                SuspendLayout();
                int limit;
                if (newCount > oldCount)
                {
                    limit = oldCount;
                    for (int j = oldCount; j < newCount; j++)
                    {
                        TableRow row = CreateRow();
                        _rowCells.Add(row);
                    }
                    for (int i = 0; i < _columns.Count; i++)
                    {
                        TableColumn<T> column = _columns[i];
                        List<TableCell> cells = _cells[i].cells;
                        for (int j = oldCount; j < newCount; j++)
                        {
                            TableCell cell = CreateCell(_items[j], column);
                            cells.Add(cell);
                            _rowCells[j].Controls.Add(cell);
                        }
                    }
                }
                else
                {
                    limit = newCount;
                    foreach (ColumnHolder col in _cells)
                    {
                        List<TableCell> cells = col.cells;
                        cells.RemoveRange(newCount, oldCount - newCount);
                    }
                    for (int i = newCount; i < oldCount; i++)
                    {
                        TableRow row = _rowCells[i];
                        if (row.Parent != null)
                        {
                            Controls.Remove(row);
                        }
                        row.Dispose();
                    }
                    _rowCells.RemoveRange(newCount, oldCount - newCount);
                }

                for (int i = 0; i < newCount; i++)
                {
                    _permutation.Add(i);
                }
                for (int i = 0; i < limit; i++)
                {
                    UpdateRow(i, _items[i]);
                }
                Sort(false);
                DoFilter();
                RestartLayout(true);

                if (_items != null)
                {
                    _items.ListChanged += ItemsChanged;
                }
            }
        }

        public Func<TableRow> RowFactory { get; set; }

        private TableRow CreateRow()
        {
            return RowFactory == null ? new TableRow() : RowFactory();
        }

        public TableColumnCollection<T> Columns => _columns;

        public override LayoutEngine LayoutEngine
        {
            get
            {
                if (_layoutEngine == null)
                {
                    _layoutEngine = new TableLayoutEngine(this);
                }
                return _layoutEngine;
            }
        }
        
        public SortOrder SortOrder
        {
            get => _sortType;
            set {
                _sortType = value;
                Sort();
            }
        }

        public ToolTip ToolTip { get; set; }
    }
}
