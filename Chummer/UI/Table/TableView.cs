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
using System.Buffers;
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
                int[] aintSharedWidths = _table._columns.Count > GlobalOptions.MaxStackLimit ? ArrayPool<int>.Shared.Rent(_table._columns.Count) : null;
                // ReSharper disable once MergeConditionalExpression
#pragma warning disable IDE0029 // Use coalesce expression
                Span<int> widths = aintSharedWidths != null ? aintSharedWidths : stackalloc int[_table._columns.Count];
#pragma warning restore IDE0029 // Use coalesce expression

                int y = 0;

                // determine minimal widths
                for (int i = 0; i < widths.Length; i++)
                {
                    Size headerMinSize = _table._lstCells[i].header.MinimumSize;
                    int width = Math.Max(_table._columns[i].MinWidth, headerMinSize.Width);
                    if (y < headerMinSize.Height)
                    {
                        y = headerMinSize.Height;
                    }
                    widths[i] = width;
                }

                for (int i = 0; i < _table._lstRowCells.Count; i++)
                {
                    TableRow row = _table._lstRowCells[i];
                    if (row.Parent != null)
                    {
                        for (int j = 0; j < widths.Length; j++)
                        {
                            int w = _table._lstCells[j].cells[i].MinimumSize.Width;
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
                    Control header = _table._lstCells[i].header;
                    header.Location = new Point(x, 0);
                    header.Size = new Size(widths[i], y);
                    x += widths[i];
                }

                int rowIndex = 0;

                // layout cells
                foreach (int index in _table._lstPermutation)
                {
                    int dy = 0;
                    x = 0;
                    TableRow row = _table._lstRowCells[index];
                    if (row.Parent != null)
                    {
                        row.SuspendLayout();
                        row.Index = rowIndex;
                        row.Selected = (_table._intSelectedIndex == index);
                        row.Location = new Point(0, y);
                        row.Width = widthSum;
                        for (int i = 0; i < _table._columns.Count; i++)
                        {
                            TableCell cell = _table._lstCells[i].cells[index];
                            cell.Location = new Point(x, row.Margin.Top);
                            if (dy < cell.Height)
                            {
                                dy = cell.Height;
                            }
                            x += widths[i];
                        }
                        for (int i = 0; i < _table._columns.Count; i++)
                        {
                            TableCell cell = _table._lstCells[i].cells[index];
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
                if (aintSharedWidths != null)
                    ArrayPool<int>.Shared.Return(aintSharedWidths);
                return true;
            }
        }

        private class ColumnHolder
        {
            public readonly HeaderCell header;
            public readonly IList<TableCell> cells;

            public ColumnHolder(HeaderCell header, IList<TableCell> cells)
            {
                this.header = header;
                this.cells = cells;
            }
        }

        private readonly Predicate<T> _defaultFilter;
        private TableColumn<T> _sortColumn;
        private BindingList<T> _lstItems;
        private Predicate<T> _filter;
        private readonly TableColumnCollection<T> _columns;
        private TableLayoutEngine _layoutEngine;
        private readonly List<ColumnHolder> _lstCells = new List<ColumnHolder>();
        private readonly Dictionary<string, List<int>> _dicObservedProperties = new Dictionary<string, List<int>>();
        private readonly List<int> _lstPermutation = new List<int>();
        private readonly List<TableRow> _lstRowCells = new List<TableRow>();
        private SortOrder _eSortType = SortOrder.None;
        private object _objSortPausedSender;
        private readonly int _intSelectedIndex = -1;

        public TableView()
        {
            _columns = new TableColumnCollection<T>(this);
            _defaultFilter = item => true;
            _filter = _defaultFilter;
            InitializeComponent();
        }

        private void ItemPropertyChanged(int intIndex, T objItem, string strProperty)
        {
            SuspendLayout();
            if (string.IsNullOrEmpty(strProperty))
            {
                // update all cells
                UpdateRow(intIndex, objItem);
            }
            else
            {
                // update cells in columns that have the column as dependency
                if (_dicObservedProperties.TryGetValue(strProperty, out List<int> lstColumns))
                {
                    foreach (int intColumnIndex in lstColumns)
                    {
                        if (intIndex >= _lstCells[intColumnIndex].cells.Count)
                            continue;
                        UpdateCell(_columns[intColumnIndex], _lstCells[intColumnIndex].cells[intIndex], objItem);
                    }
                }
            }
            ResumeLayout(true);
        }

        public void PauseSort(object objSender)
        {
            _objSortPausedSender = objSender;
        }

        public void ResumeSort(object objSender)
        {
            if (_objSortPausedSender == objSender)
            {
                _objSortPausedSender = null;

                // prevent sort for focus loss when disposing
                if (_lstItems != null && _lstPermutation.Count == _lstItems.Count)
                {
                    Sort();
                }
            }
        }

        private void Sort(bool blnPerformLayout = true)
        {
            if (_objSortPausedSender != null) return;
            if (_eSortType == SortOrder.None || _sortColumn == null)
            {
                // sort by index
                _lstPermutation.Sort();
            }
            else
            {
                Comparison<T> comparison = _sortColumn.CreateSorter();

                _lstPermutation.Sort((i1, i2) => comparison(_lstItems[i1], _lstItems[i2]));
                if (_eSortType == SortOrder.Descending)
                {
                    _lstPermutation.Reverse();
                }
            }
            if (blnPerformLayout)
            {
                PerformLayout();
            }
        }

        private void UpdateCell(TableColumn<T> column, TableCell cell, T item)
        {
            Func<T, object> funcExtractor = column.Extractor;
            cell.UpdateValue(funcExtractor == null ? item : funcExtractor(item));
            Func<T, string> funcTooltipExtractor = column.ToolTipExtractor;
            ToolTip tooltip = ToolTip;
            if (tooltip != null && funcTooltipExtractor != null)
            {
                Control content = cell.Content;
                string strText = funcTooltipExtractor(item);
                tooltip.SetToolTip(content ?? cell, strText);
            }
        }

        private void UpdateRow(int index, T item)
        {
            for (int i = 0; i < _columns.Count; i++)
            {
                IList<TableCell> cells = _lstCells[i].cells;
                if (index >= cells.Count)
                    continue;
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
            if (_lstItems != null)
            {
                cells = new List<TableCell>(_lstItems.Count);
                for (int i = 0; i < _lstItems.Count; i++)
                {
                    T item = _lstItems[i];
                    TableCell cell = CreateCell(item, column);
                    cells.Add(cell);
                    if (_filter(item))
                    {
                        TableRow row = _lstRowCells[i];
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
            HeaderCell header = new HeaderCell
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
                        switch(_eSortType)
                        {
                            case SortOrder.None:
                                _eSortType = SortOrder.Ascending;
                                break;
                            case SortOrder.Ascending:
                                _eSortType = SortOrder.Descending;
                                break;
                            case SortOrder.Descending:
                                _eSortType = SortOrder.None;
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
                                    _lstCells[i].header.SortType = SortOrder.None;
                                    break;
                                }
                            }
                        }
                        _sortColumn = column;
                        _eSortType = SortOrder.Ascending;
                    }
                    header.SortType = _eSortType;
                    Sort();
                };
                header.Sortable = true;
            }
            Controls.Add(header);
            _lstCells.Insert(insertIndex, new ColumnHolder(header, cells));
            ResumeLayout(false);
        }

        internal void ColumnAdded(TableColumn<T> column) {
            column.MakeLive();
            int index = _columns.Count - 1;
            CreateCellsForColumn(index, column);
            foreach (string dependency in column.Dependencies)
            {
                if (!_dicObservedProperties.TryGetValue(dependency, out List<int> lstDependencies))
                {
                    lstDependencies = new List<int>();
                    _dicObservedProperties[dependency] = lstDependencies;
                }
                lstDependencies.Add(index);
            }
        }

        private void DisposeAll()
        {
            _lstPermutation.Clear();
            foreach (ColumnHolder col in _lstCells)
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
            if (_lstItems == null) return;
            SuspendLayout();

            for (int i = 0; i < _lstItems.Count; i++)
            {
                TableRow row = _lstRowCells[i];
                row.SuspendLayout();
                if (_filter(_lstItems[i]))
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
                    T item = _lstItems[e.NewIndex];
                    if (e.PropertyDescriptor == null)
                    {
                        SuspendLayout();
                        row = _lstRowCells[e.NewIndex];
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
                    item = _lstItems[e.NewIndex];
                    row = CreateRow();
                    _lstRowCells.Insert(e.NewIndex, row);
                    SuspendLayout();
                    if (_filter(item))
                    {
                        Controls.Add(row);
                    }
                    row.SuspendLayout();
                    for (int i = 0; i < _columns.Count; i++)
                    {
                        TableColumn<T> column = _columns[i];
                        IList<TableCell> cells = _lstCells[i].cells;
                        TableCell newCell = CreateCell(item, column);
                        cells.Insert(e.NewIndex, newCell);
                        row.Controls.Add(newCell);
                    }
                    row.ResumeLayout(false);
                    _lstPermutation.Add(_lstPermutation.Count);
                    Sort(false);
                    RestartLayout(true);
                    break;
                case ListChangedType.ItemDeleted:
                    SuspendLayout();
                    for (int i = 0; i < _columns.Count; i++)
                    {
                        IList<TableCell> cells = _lstCells[i].cells;
                        cells.RemoveAt(e.NewIndex);
                    }
                    row = _lstRowCells[e.NewIndex];
                    if (row.Parent != null)
                    {
                        Controls.Remove(row);
                    }
                    row.Dispose();
                    _lstRowCells.RemoveAt(e.NewIndex);
                    _lstPermutation.Remove(_lstPermutation.Count - 1);
                    Sort(false);
                    RestartLayout(true);
                    break;
                case ListChangedType.ItemMoved:
                    foreach (ColumnHolder col in _lstCells)
                    {
                        IList<TableCell> cells = col.cells;
                        TableCell cell = cells[e.OldIndex];
                        cells.RemoveAt(e.OldIndex);
                        cells.Insert(e.NewIndex, cell);
                    }
                    row = _lstRowCells[e.OldIndex];
                    _lstRowCells.RemoveAt(e.OldIndex);
                    _lstRowCells.Insert(e.NewIndex, row);

                    // fix permutation
                    int intMinIndex, intMaxIndex, intDelta;
                    if (e.OldIndex < e.NewIndex)
                    {
                        intDelta = -1;
                        intMinIndex = e.OldIndex + 1;
                        intMaxIndex = e.NewIndex;
                    }
                    else
                    {
                        intDelta = +1;
                        intMinIndex = e.NewIndex;
                        intMaxIndex = e.OldIndex - 1;
                    }
                    for (int i = 0; i < _lstPermutation.Count; i++)
                    {
                        int value = _lstPermutation[i];
                        if (value == e.OldIndex)
                        {
                            _lstPermutation[i] = e.NewIndex;
                        }
                        else if (value >= intMinIndex && value <= intMaxIndex)
                        {
                            _lstPermutation[i] = value + intDelta;
                        }
                    }
                    Sort();
                    break;
            }
        }

        /// <summary>
        /// The list of items displayed in the table.
        /// </summary>
        public BindingList<T> Items
        {
            get => _lstItems;
            set
            {
                int intOldCount = 0;
                if (_lstItems != null)
                {
                    // remove listener from old items
                    _lstItems.ListChanged -= ItemsChanged;
                    intOldCount = _lstItems.Count;
                    _lstPermutation.Clear();
                }

                _lstItems = value;
                int intNewCount = _lstItems?.Count ?? 0;
                SuspendLayout();
                int intLimit;
                if (intNewCount > intOldCount)
                {
                    intLimit = intOldCount;
                    for (int j = intOldCount; j < intNewCount; j++)
                    {
                        TableRow row = CreateRow();
                        _lstRowCells.Add(row);
                    }
                    for (int i = 0; i < _columns.Count; i++)
                    {
                        TableColumn<T> column = _columns[i];
                        IList<TableCell> cells = _lstCells[i].cells;
                        for (int j = intOldCount; j < intNewCount; j++)
                        {
                            TableCell cell = CreateCell(_lstItems[j], column);
                            cells.Add(cell);
                            _lstRowCells[j].Controls.Add(cell);
                        }
                    }
                }
                else
                {
                    intLimit = intNewCount;
                    foreach (ColumnHolder col in _lstCells)
                    {
                        IList<TableCell> cells = col.cells;
                        cells.RemoveRange(intNewCount, intOldCount - intNewCount);
                    }
                    for (int i = intNewCount; i < intOldCount; i++)
                    {
                        TableRow row = _lstRowCells[i];
                        if (row.Parent != null)
                        {
                            Controls.Remove(row);
                        }
                        row.Dispose();
                    }
                    _lstRowCells.RemoveRange(intNewCount, intOldCount - intNewCount);
                }

                for (int i = 0; i < intNewCount; i++)
                {
                    _lstPermutation.Add(i);
                }
                for (int i = 0; i < intLimit; i++)
                {
                    UpdateRow(i, _lstItems[i]);
                }
                Sort(false);
                DoFilter();
                RestartLayout(true);

                if (_lstItems != null)
                {
                    _lstItems.ListChanged += ItemsChanged;
                }
            }
        }

        public Func<TableRow> RowFactory { get; set; }

        private TableRow CreateRow()
        {
            return RowFactory == null ? new TableRow() : RowFactory();
        }

        public TableColumnCollection<T> Columns => _columns;

        public override LayoutEngine LayoutEngine => _layoutEngine = _layoutEngine ?? new TableLayoutEngine(this);

        public SortOrder SortOrder
        {
            get => _eSortType;
            set {
                _eSortType = value;
                Sort();
            }
        }

        public ToolTip ToolTip { get; set; }
    }
}
