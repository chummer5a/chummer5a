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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
                using (CursorWait.New(_table))
                {
                    _table.SuspendLayout();
                    try
                    {
                        int intCount = _table._columns.Count;
                        int[] aintSharedWidths = intCount > GlobalSettings.MaxStackLimit
                            ? ArrayPool<int>.Shared.Rent(intCount)
                            : null;
                        try
                        {
                            // ReSharper disable once MergeConditionalExpression
#pragma warning disable IDE0029 // Use coalesce expression
                            Span<int> widths = aintSharedWidths != null
                                ? aintSharedWidths
                                : stackalloc int[intCount];
#pragma warning restore IDE0029 // Use coalesce expression

                            int y = 0;

                            // determine minimal widths
                            for (int i = 0; i < intCount; i++)
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
                                x = 0;
                                TableRow row = _table._lstRowCells[index];
                                if (row.Parent != null)
                                {
                                    row.SuspendLayout();
                                    try
                                    {
                                        row.Index = rowIndex;
                                        row.Selected = _table._intSelectedIndex == index;
                                        row.Location = new Point(0, y);
                                        row.Width = widthSum;
                                        int dy = 0;
                                        for (int i = 0; i < _table._columns.Count; i++)
                                        {
                                            TableCell cell = _table._lstCells[i].cells[index];
                                            cell.Location = new Point(x, row.Margin.Top);
                                            int intHeight = Math.Max(cell.Height, cell.PreferredSize.Height);
                                            if (dy < intHeight)
                                            {
                                                dy = intHeight;
                                            }

                                            x += widths[i];
                                        }

                                        for (int i = 0; i < _table._columns.Count; i++)
                                        {
                                            TableCell cell = _table._lstCells[i].cells[index];
                                            cell.UpdateAvailableSize(widths[i], dy);
                                        }

                                        row.Height = dy + row.Margin.Top + row.Margin.Bottom;
                                    }
                                    finally
                                    {
                                        row.ResumeLayout(false);
                                    }

                                    y += row.Height;
                                    rowIndex++;
                                }
                            }

                            if (y > _table.Height)
                            {
                                _table.Height = y;
                            }
                        }
                        finally
                        {
                            if (aintSharedWidths != null)
                                ArrayPool<int>.Shared.Return(aintSharedWidths);
                        }
                    }
                    finally
                    {
                        _table.ResumeLayout(false);
                    }
                }

                return true;
            }
        }

        private sealed class ColumnHolder
        {
            public readonly HeaderCell header;
            public readonly IList<TableCell> cells;

            public ColumnHolder(HeaderCell header, IList<TableCell> cells)
            {
                this.header = header;
                this.cells = cells;
            }
        }

        private readonly Func<T, Task<bool>> _funcDefaultFilter;
        private TableColumn<T> _sortColumn;
        private ThreadSafeBindingList<T> _lstItems;
        private readonly AsyncFriendlyReaderWriterLock _objItemsLocker = new AsyncFriendlyReaderWriterLock();
        private Func<T, Task<bool>> _funcFilter;
        private readonly TableColumnCollection<T> _columns;
        private TableLayoutEngine _layoutEngine;
        private readonly List<ColumnHolder> _lstCells = new List<ColumnHolder>();
        private readonly ConcurrentDictionary<string, List<int>> _dicObservedProperties = new ConcurrentDictionary<string, List<int>>();
        private readonly List<int> _lstPermutation = new List<int>();
        private readonly List<TableRow> _lstRowCells = new List<TableRow>();
        private SortOrder _eSortType = SortOrder.None;
        private object _objSortPausedSender;
        private readonly int _intSelectedIndex = -1;

        public TableView()
        {
            _columns = new TableColumnCollection<T>(this);
            _funcDefaultFilter = x => Task.FromResult(true);
            _funcFilter = _funcDefaultFilter;
            InitializeComponent();
            Disposed += (sender, args) => DisposeAll();
        }

        private async Task ItemPropertyChanged(int intIndex, T objItem, string strProperty, CancellationToken token = default)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                await this.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                try
                {
                    if (string.IsNullOrEmpty(strProperty))
                    {
                        // update all cells
                        await UpdateRow(intIndex, objItem, token).ConfigureAwait(false);
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
                                await UpdateCell(_columns[intColumnIndex], _lstCells[intColumnIndex].cells[intIndex],
                                                 objItem, token).ConfigureAwait(false);
                            }
                        }
                    }
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout(true), token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        public void PauseSort(object objSender)
        {
            _objSortPausedSender = objSender;
        }

        public void ResumeSort(object objSender, CancellationToken token = default)
        {
            if (Interlocked.CompareExchange(ref _objSortPausedSender, null, objSender) == objSender
                // prevent sort for focus loss when disposing
                && Items != null && _lstPermutation.Count == Items.Count)
            {
                Sort(token: token);
            }
        }

        public async Task ResumeSortAsync(object objSender, CancellationToken token = default)
        {
            if (Interlocked.CompareExchange(ref _objSortPausedSender, null, objSender) == objSender
                // prevent sort for focus loss when disposing
                && Items != null && _lstPermutation.Count == await Items.GetCountAsync(token).ConfigureAwait(false))
            {
                await SortAsync(token: token).ConfigureAwait(false);
            }
        }

        private void Sort(bool blnPerformLayout = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_objSortPausedSender != null)
                return;
            if (_eSortType == SortOrder.None || _sortColumn == null)
            {
                // sort by index
                _lstPermutation.Sort();
            }
            else
            {
                Func<T, T, Task<int>> comparison = _sortColumn.CreateSorter();

                _lstPermutation.Sort((i1, i2) => Utils.SafelyRunSynchronously(
                                         async () => await comparison(
                                                 await Items.GetValueAtAsync(i1, token).ConfigureAwait(false),
                                                 await Items.GetValueAtAsync(i2, token).ConfigureAwait(false))
                                             .ConfigureAwait(false), token));
                if (_eSortType == SortOrder.Descending)
                {
                    _lstPermutation.Reverse();
                }
            }
            if (blnPerformLayout)
            {
                this.DoThreadSafe((x, y) => x.PerformLayout(), token);
            }
        }

        private async Task SortAsync(bool blnPerformLayout = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_objSortPausedSender != null)
                return;
            if (_eSortType == SortOrder.None || _sortColumn == null)
            {
                // sort by index
                _lstPermutation.Sort();
            }
            else
            {
                Func<T, T, Task<int>> comparison = _sortColumn.CreateSorter();

                await _lstPermutation.SortAsync(async (i1, i2) => await comparison(
                        await Items.GetValueAtAsync(i1, token).ConfigureAwait(false),
                        await Items.GetValueAtAsync(i2, token).ConfigureAwait(false))
                    .ConfigureAwait(false), token: token).ConfigureAwait(false);
                if (_eSortType == SortOrder.Descending)
                {
                    _lstPermutation.Reverse();
                }
            }
            if (blnPerformLayout)
            {
                await this.DoThreadSafeAsync((x, y) => x.PerformLayout(), token).ConfigureAwait(false);
            }
        }

        private async Task UpdateCell(TableColumn<T> column, TableCell cell, T item, CancellationToken token = default)
        {
            Func<T, Task<object>> funcExtractor = column.Extractor;
            object objNewValue = funcExtractor == null ? item : await funcExtractor(item).ConfigureAwait(false);
            await cell.UpdateValueAsync(objNewValue, token).ConfigureAwait(false);
            Func<T, Task<string>> funcTooltipExtractor = column.ToolTipExtractor;
            if (funcTooltipExtractor != null)
            {
                Control content = await cell.DoThreadSafeFuncAsync(x => x.Content, token: token).ConfigureAwait(false);
                string strText = await funcTooltipExtractor(item).ConfigureAwait(false);
                await (content ?? cell).SetToolTipAsync(strText, token).ConfigureAwait(false);
            }
        }

        private async Task UpdateRow(int index, T item, CancellationToken token = default)
        {
            for (int i = 0; i < _columns.Count; i++)
            {
                IList<TableCell> cells = _lstCells[i].cells;
                if (index >= cells.Count)
                    continue;
                TableColumn<T> column = _columns[i];
                TableCell cell = cells[index];
                await UpdateCell(column, cell, item, token).ConfigureAwait(false);
            }
        }

        private async Task<TableCell> CreateCell(T item, TableColumn<T> column, CancellationToken token = default)
        {
            TableCell cell = await this.DoThreadSafeFuncAsync(column.CreateCell, token: token).ConfigureAwait(false);
            await UpdateCell(column, cell, item, token).ConfigureAwait(false);
            return cell;
        }

        private async Task CreateCellsForColumn(int insertIndex, TableColumn<T> column, CancellationToken token = default)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                await this.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                try
                {
                    List<TableCell> cells;
                    if (Items != null)
                    {
                        cells = new List<TableCell>(await Items.GetCountAsync(token).ConfigureAwait(false));
                        int i = 0;
                        await Items.ForEachAsync(async item =>
                        {
                            TableCell cell = await CreateCell(item, column, token).ConfigureAwait(false);
                            cells.Add(cell);
                            if (await Filter(item).ConfigureAwait(false))
                            {
                                TableRow row = _lstRowCells[i++];
                                await row.DoThreadSafeAsync(x =>
                                {
                                    x.SuspendLayout();
                                    try
                                    {
                                        x.Controls.Add(cell);
                                    }
                                    finally
                                    {
                                        x.ResumeLayout(false);
                                    }
                                }, token).ConfigureAwait(false);
                            }
                        }, token: token).ConfigureAwait(false);
                    }
                    else
                    {
                        cells = new List<TableCell>(1);
                    }

                    HeaderCell header = await this.DoThreadSafeFuncAsync(() => new HeaderCell
                    {
                        Text = column.Text,
                        TextTag = column.Tag
                    }, token: token).ConfigureAwait(false);
                    if (column.Sorter != null)
                    {
                        header.HeaderClick += SortOnClick;
                        header.Sortable = true;

                        async void SortOnClick(object sender, EventArgs evt)
                        {
                            if (_sortColumn == column)
                            {
                                // cycle through states if column remains the same
                                switch (_eSortType)
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
                            // ReSharper disable once MethodSupportsCancellation
                            await SortAsync(token: CancellationToken.None).ConfigureAwait(false);
                        }
                    }

                    await this.DoThreadSafeAsync(x => x.Controls.Add(header), token).ConfigureAwait(false);
                    _lstCells.Insert(insertIndex, new ColumnHolder(header, cells));
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout(false), token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        internal async Task ColumnAdded(TableColumn<T> column, CancellationToken token = default)
        {
            column.MakeLive();
            int index = _columns.Count - 1;
            await CreateCellsForColumn(index, column, token).ConfigureAwait(false);
            foreach (string dependency in column.Dependencies)
            {
                List<int> lstDependencies = _dicObservedProperties.GetOrAdd(dependency, x => new List<int>(1));
                lstDependencies.Add(index);
            }
        }

        private void DisposeAll()
        {
            _objItemsLocker.Dispose();
            _lstPermutation.Clear();
            foreach (TableCell cell in _lstCells.SelectMany(col => col.cells))
                cell.Dispose();
            foreach (TableColumn<T> objColumn in Columns)
                objColumn.Dispose();
            Controls.Clear();
        }

        private async Task DoFilter(bool performLayout = true, CancellationToken token = default)
        {
            if (Items == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                await this.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                try
                {
                    int i = 0;
                    await Items.ForEachAsync(async objItem =>
                    {
                        TableRow row = _lstRowCells[i++];
                        await row.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                        try
                        {
                            if (await Filter(objItem).ConfigureAwait(false))
                            {
                                if (await row.DoThreadSafeFuncAsync(x => x.Parent, token: token).ConfigureAwait(false)
                                    == null)
                                {
                                    await this.DoThreadSafeAsync(x => x.Controls.Add(row), token).ConfigureAwait(false);
                                }
                            }
                            else if (await row.DoThreadSafeFuncAsync(x => x.Parent, token: token).ConfigureAwait(false)
                                     != null)
                            {
                                await this.DoThreadSafeAsync(x => x.Controls.Remove(row), token).ConfigureAwait(false);
                            }
                        }
                        finally
                        {
                            await row.DoThreadSafeAsync(x => x.ResumeLayout(false), token).ConfigureAwait(false);
                        }
                    }, token: token).ConfigureAwait(false);
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.RestartLayout(performLayout), token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private void RestartLayout(bool performLayout)
        {
            ResumeLayout(performLayout);
        }

        /// <summary>
        /// Predicate for filtering the items.
        /// </summary>
        public Func<T, Task<bool>> Filter => _funcFilter;

        public async Task SetFilterAsync(Func<T, Task<bool>> value, CancellationToken token = default)
        {
            Func<T, Task<bool>> objNewValue = value ?? _funcDefaultFilter;
            if (Interlocked.Exchange(ref _funcFilter, objNewValue) == objNewValue)
                return;
            await DoFilter(token: token).ConfigureAwait(false);
        }

        private async Task ItemsChanged(object sender, ListChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemChanged:
                {
                    T item = await Items.GetValueAtAsync(e.NewIndex, token).ConfigureAwait(false);
                    if (e.PropertyDescriptor == null)
                    {
                        CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                        try
                        {
                            await this.DoThreadSafeAsync(x => x.SuspendLayout(), token: token).ConfigureAwait(false);
                            try
                            {
                                TableRow row = _lstRowCells[e.NewIndex];
                                if (await Filter(item).ConfigureAwait(false))
                                {
                                    if (row.Parent == null)
                                    {
                                        await this.DoThreadSafeAsync(x => x.Controls.Add(row), token: token).ConfigureAwait(false);
                                    }
                                }
                                else if (row.Parent != null)
                                {
                                    await this.DoThreadSafeAsync(x => x.Controls.Remove(row), token: token).ConfigureAwait(false);
                                }

                                await UpdateRow(e.NewIndex, item, token).ConfigureAwait(false);
                                await SortAsync(false, token).ConfigureAwait(false);
                            }
                            finally
                            {
                                await this.DoThreadSafeAsync(x => x.RestartLayout(true), token: token).ConfigureAwait(false);
                            }
                        }
                        finally
                        {
                            await objCursorWait.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        await ItemPropertyChanged(e.NewIndex, item, e.PropertyDescriptor.Name, token).ConfigureAwait(false);
                    }

                    break;
                }

                case ListChangedType.ItemAdded:
                {
                    T item = await Items.GetValueAtAsync(e.NewIndex, token).ConfigureAwait(false);
                    Control[] lstToAdd = new Control[_columns.Count];
                    for (int i = 0; i < _columns.Count; i++)
                    {
                        TableColumn<T> column = _columns[i];
                        IList<TableCell> cells = _lstCells[i].cells;
                        TableCell newCell = await CreateCell(item, column, token).ConfigureAwait(false);
                        cells.Insert(e.NewIndex, newCell);
                        lstToAdd[i] = newCell;
                    }

                    TableRow row = await this.DoThreadSafeFuncAsync(x => x.CreateRow(), token: token).ConfigureAwait(false);
                    _lstRowCells.Insert(e.NewIndex, row);
                    CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                    try
                    {
                        await this.DoThreadSafeAsync(x => x.SuspendLayout(), token: token).ConfigureAwait(false);
                        try
                        {
                            if (await Filter(item).ConfigureAwait(false))
                            {
                                await this.DoThreadSafeAsync(x => x.Controls.Add(row), token: token).ConfigureAwait(false);
                            }

                            await row.DoThreadSafeAsync(x =>
                            {
                                x.SuspendLayout();
                                try
                                {
                                    x.Controls.AddRange(lstToAdd);
                                }
                                finally
                                {
                                    x.ResumeLayout(false);
                                }
                            }, token: token).ConfigureAwait(false);

                            _lstPermutation.Add(_lstPermutation.Count);
                            await SortAsync(false, token).ConfigureAwait(false);
                        }
                        finally
                        {
                            await this.DoThreadSafeAsync(x => x.RestartLayout(true), token: token).ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        await objCursorWait.DisposeAsync().ConfigureAwait(false);
                    }

                    break;
                }
                case ListChangedType.ItemDeleted:
                {
                    for (int i = 0; i < _columns.Count; i++)
                    {
                        IList<TableCell> cells = _lstCells[i].cells;
                        cells.RemoveAt(e.NewIndex);
                    }

                    CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                    try
                    {
                        await this.DoThreadSafeAsync(x => x.SuspendLayout(), token: token).ConfigureAwait(false);
                        try
                        {
                            await this.DoThreadSafeAsync(x =>
                            {
                                TableRow row = x._lstRowCells[e.NewIndex];
                                if (row.Parent != null)
                                {
                                    x.Controls.Remove(row);
                                }

                                row.Dispose();
                            }, token: token).ConfigureAwait(false);

                            _lstRowCells.RemoveAt(e.NewIndex);
                            _lstPermutation.Remove(_lstPermutation.Count - 1);
                            await SortAsync(false, token).ConfigureAwait(false);
                        }
                        finally
                        {
                            await this.DoThreadSafeAsync(x => x.RestartLayout(true), token: token).ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        await objCursorWait.DisposeAsync().ConfigureAwait(false);
                    }

                    break;
                }
                case ListChangedType.ItemMoved:
                {
                    foreach (IList<TableCell> cells in _lstCells.Select(x => x.cells))
                    {
                        TableCell cell = cells[e.OldIndex];
                        cells.RemoveAt(e.OldIndex);
                        cells.Insert(e.NewIndex, cell);
                    }

                    TableRow row = _lstRowCells[e.OldIndex];
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

                    await SortAsync(token: token).ConfigureAwait(false);
                    break;
                }
            }
        }

        /// <summary>
        /// The list of items displayed in the table.
        /// </summary>
        public ThreadSafeBindingList<T> Items
        {
            get
            {
                using (_objItemsLocker.EnterReadLock())
                    return _lstItems;
            }
            set
            {
                using (_objItemsLocker.EnterUpgradeableReadLock())
                {
                    int intOldCount = 0;
                    ThreadSafeBindingList<T> lstOldItems = Interlocked.Exchange(ref _lstItems, value);
                    if (lstOldItems != null)
                    {
                        // remove listener from old items
                        lstOldItems.ListChangedAsync -= ItemsChanged;
                        intOldCount = lstOldItems.Count;
                    }

                    using (_objItemsLocker.EnterWriteLock())
                    {
                        if (lstOldItems != null)
                            _lstPermutation.Clear();
                        int intNewCount = value?.Count ?? 0;
                        using (CursorWait.New(this))
                        {
                            SuspendLayout();
                            try
                            {
                                int intLimit;
                                if (intNewCount > intOldCount && value != null)
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
                                            int j1 = j;
                                            TableCell cell
                                                = Utils.SafelyRunSynchronously(() => CreateCell(value[j1], column));
                                            cells.Add(cell);
                                            _lstRowCells[j1].Controls.Add(cell);
                                        }
                                    }
                                }
                                else
                                {
                                    intLimit = intNewCount;
                                    foreach (IList<TableCell> cells in _lstCells.Select(x => x.cells))
                                    {
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

                                if (value != null)
                                {
                                    for (int i = 0; i < intLimit; i++)
                                    {
                                        int i1 = i;
                                        Utils.SafelyRunSynchronously(() => UpdateRow(i1, value[i1]));
                                    }
                                }

                                Sort(false);
                                Utils.SafelyRunSynchronously(() => DoFilter());
                            }
                            finally
                            {
                                RestartLayout(true);
                            }
                        }

                        if (value != null)
                        {
                            value.ListChangedAsync += ItemsChanged;
                        }
                    }
                }
            }
        }

        public async Task SetItemsAsync(ThreadSafeBindingList<T> value, CancellationToken token = default)
        {
            using (await _objItemsLocker.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                int intOldCount = 0;
                ThreadSafeBindingList<T> lstOldItems = Interlocked.Exchange(ref _lstItems, value);
                if (lstOldItems != null)
                {
                    // remove listener from old items
                    IAsyncDisposable objLocker2 = await lstOldItems.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        lstOldItems.ListChangedAsync -= ItemsChanged;
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }

                    intOldCount = lstOldItems.Count;
                }

                IAsyncDisposable objLocker = await _objItemsLocker.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (lstOldItems != null)
                        _lstPermutation.Clear();
                    int intNewCount = value != null ? await value.GetCountAsync(token).ConfigureAwait(false) : 0;
                    CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                    try
                    {
                        await this.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                        try
                        {
                            int intLimit;
                            if (intNewCount > intOldCount && value != null)
                            {
                                intLimit = intOldCount;
                                for (int j = intOldCount; j < intNewCount; j++)
                                {
                                    TableRow row = await this.DoThreadSafeFuncAsync(x => x.CreateRow(), token).ConfigureAwait(false);
                                    _lstRowCells.Add(row);
                                }

                                for (int i = 0; i < _columns.Count; i++)
                                {
                                    TableColumn<T> column = _columns[i];
                                    IList<TableCell> cells = _lstCells[i].cells;
                                    for (int j = intOldCount; j < intNewCount; j++)
                                    {
                                        TableCell cell = await CreateCell(value[j], column, token).ConfigureAwait(false);
                                        cells.Add(cell);
                                        await _lstRowCells[j].DoThreadSafeAsync(x => x.Controls.Add(cell), token: token).ConfigureAwait(false);
                                    }
                                }
                            }
                            else
                            {
                                intLimit = intNewCount;
                                foreach (IList<TableCell> cells in _lstCells.Select(x => x.cells))
                                {
                                    cells.RemoveRange(intNewCount, intOldCount - intNewCount, token: token);
                                }

                                for (int i = intNewCount; i < intOldCount; i++)
                                {
                                    TableRow row = _lstRowCells[i];
                                    if (row.Parent != null)
                                    {
                                        // ReSharper disable once AccessToDisposedClosure
                                        await this.DoThreadSafeAsync(x => x.Controls.Remove(row), token: token).ConfigureAwait(false);
                                    }

                                    await row.DoThreadSafeAsync(x => x.Dispose(), token).ConfigureAwait(false);
                                }

                                _lstRowCells.RemoveRange(intNewCount, intOldCount - intNewCount);
                            }

                            for (int i = 0; i < intNewCount; i++)
                            {
                                _lstPermutation.Add(i);
                            }

                            if (value != null)
                            {
                                for (int i = 0; i < intLimit; i++)
                                {
                                    await UpdateRow(i, value[i], token).ConfigureAwait(false);
                                }
                            }

                            await SortAsync(false, token).ConfigureAwait(false);
                            await DoFilter(token: token).ConfigureAwait(false);
                        }
                        finally
                        {
                            await this.DoThreadSafeAsync(x => x.RestartLayout(true), token: token).ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        await objCursorWait.DisposeAsync().ConfigureAwait(false);
                    }

                    if (value != null)
                    {
                        IAsyncDisposable objLocker2 = await value.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                        try
                        {
                            value.ListChangedAsync += ItemsChanged;
                        }
                        finally
                        {
                            await objLocker2.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
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
            set
            {
                if (InterlockedExtensions.Exchange(ref _eSortType, value) != value)
                    Sort();
            }
        }

        public async Task SetSortOrderAsync(SortOrder value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (InterlockedExtensions.Exchange(ref _eSortType, value) != value)
                await SortAsync(token: token).ConfigureAwait(false);
        }
    }
}
