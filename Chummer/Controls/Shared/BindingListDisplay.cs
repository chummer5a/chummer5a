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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;

namespace Chummer.Controls.Shared
{
    public partial class BindingListDisplay<TType> : UserControl where TType : INotifyPropertyChanged
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BindingListDisplay<TType>));
        public PropertyChangedEventHandler ChildPropertyChanged { get; set; }
        public PropertyChangedAsyncEventHandler ChildPropertyChangedAsync { get; set; }

        private int _intSuspendLayoutCount;
        private readonly Func<TType, Control> _funcCreateControl;  //Function to create a control out of a item
        private readonly bool _blnLoadVisibleOnly;
        private readonly List<ControlWithMetaData> _lstContentList = new List<ControlWithMetaData>(10);
        private readonly List<int> _lstDisplayIndex = new List<int>(10);
        private readonly IndexComparer _indexComparer;
        private bool[] _ablnRendered; // Not BitArray because read/write performance is much more important here than memory footprint
        private int _intOffScreenChunkSize = 1;
        private int _intListItemControlHeight;
        private bool _blnAllRendered;
        private Predicate<TType> _visibleFilter = x => true;
        private IComparer<TType> _comparison;

        public BindingListDisplay(ThreadSafeBindingList<TType> contents, Func<TType, Control> funcCreateControl, bool blnLoadVisibleOnly = true)
        {
            InitializeComponent();
            Disposed += (sender, args) =>
            {
                foreach (ControlWithMetaData _objControlWithMetaData in _lstContentList)
                {
                    _objControlWithMetaData.Dispose();
                }
            };
            Contents = contents ?? throw new ArgumentNullException(nameof(contents));
            _funcCreateControl = funcCreateControl;
            _blnLoadVisibleOnly = blnLoadVisibleOnly;
            if (Interlocked.Increment(ref _intSuspendLayoutCount) == 1)
                pnlDisplay.SuspendLayout();
            try
            {
                int intMaxControlHeight = 0;
                foreach (TType objLoopTType in Contents)
                {
                    ControlWithMetaData objNewControl = new ControlWithMetaData(objLoopTType, this, false);
                    intMaxControlHeight = Math.Max(objNewControl.Control.PreferredSize.Height, intMaxControlHeight);
                    _lstContentList.Add(objNewControl);
                }

                if (intMaxControlHeight > 0)
                    ListItemControlHeight = intMaxControlHeight;

                pnlDisplay.Controls.AddRange(_lstContentList.Select(x => x.Control).ToArray());
                _indexComparer = new IndexComparer(Contents);
                _comparison = _comparison ?? _indexComparer;
                Contents.ListChangedAsync += ContentsChanged;
                Disposed += (sender, args) =>
                {
                    try
                    {
                        Contents.ListChangedAsync -= ContentsChanged;
                    }
                    catch (ObjectDisposedException)
                    {
                        //swallow this
                    }
                };
                ComputeDisplayIndex();
                LoadScreenContent();
                BindingListDisplay_SizeChanged(this, EventArgs.Empty);
            }
            finally
            {
                if (Interlocked.Decrement(ref _intSuspendLayoutCount) == 0)
                    pnlDisplay.ResumeLayout();
            }
        }

        private void BindingListDisplay_Load(object sender, EventArgs e)
        {
            Application.Idle += ApplicationOnIdle;
            Disposed += (o, args) => Application.Idle -= ApplicationOnIdle;
        }

        /// <summary>
        /// Base BindingList that represents all possible contents of the display, not necessarily all visible.
        /// </summary>
        public ThreadSafeBindingList<TType> Contents { get; }

        public IEnumerable<Control> ContentControls => _lstContentList.Select(x => x.Control);

        public Panel DisplayPanel => pnlDisplay;

        private int ListItemControlHeight
        {
            get => _intListItemControlHeight;
            set
            {
                if (Interlocked.Exchange(ref _intListItemControlHeight, value) == value)
                    return;
                foreach (ControlWithMetaData objControl in _lstContentList)
                {
                    objControl.UpdateHeight();
                }
            }
        }

        private void LoadRange(int min, int max)
        {
            min = Math.Max(0, min);
            max = Math.Min(_lstDisplayIndex.Count, max);
            if (_ablnRendered.FirstMatching(false, min) > max)
                return;
            if (Interlocked.Increment(ref _intSuspendLayoutCount) == 1)
                pnlDisplay.DoThreadSafe(x => x.SuspendLayout());
            try
            {
                for (int i = min; i < max; ++i)
                {
                    if (_ablnRendered[i])
                        continue;

                    ControlWithMetaData item = _lstContentList[_lstDisplayIndex[i]];
                    int intLocal = i;
                    item.Control.DoThreadSafe(x =>
                    {
                        x.Location = new Point(0, intLocal * ListItemControlHeight);
                        x.Visible = true;
                    });
                    _ablnRendered[i] = true;
                }
            }
            finally
            {
                if (Interlocked.Decrement(ref _intSuspendLayoutCount) == 0)
                    pnlDisplay.DoThreadSafe(x => x.ResumeLayout());
            }
        }

        private void ComputeDisplayIndex()
        {
            List<Tuple<TType, int>> objTTypeList = new List<Tuple<TType, int>>(_lstContentList.Count);
            for (int i = 0; i < _lstContentList.Count; ++i)
            {
                ControlWithMetaData objLoopControl = _lstContentList[i];
                if (objLoopControl.Visible)
                {
                    objTTypeList.Add(new Tuple<TType, int>(objLoopControl.Item, i));
                }
            }

            objTTypeList.Sort((x, y) => _comparison.Compare(x.Item1, y.Item1));

            // Array is temporary and of primitives, so stackalloc used instead of List.ToArray() (which would put the array on the heap) when possible
            int[] aintSharedOldDisplayIndexes = _lstDisplayIndex.Count > GlobalSettings.MaxStackLimit
                ? ArrayPool<int>.Shared.Rent(_lstDisplayIndex.Count)
                : null;
            try
            {
                // ReSharper disable once MergeConditionalExpression
#pragma warning disable IDE0029 // Use coalesce expression
                Span<int> aintOldDisplayIndex = aintSharedOldDisplayIndexes != null
                    ? aintSharedOldDisplayIndexes
                    : stackalloc int[_lstDisplayIndex.Count];
#pragma warning restore IDE0029 // Use coalesce expression
                for (int i = 0; i < _lstDisplayIndex.Count; ++i)
                    aintOldDisplayIndex[i] = _lstDisplayIndex[i];
                _lstDisplayIndex.Clear();
                _lstDisplayIndex.AddRange(objTTypeList.Select(x => x.Item2));

                if (_ablnRendered == null || _ablnRendered.Length != _lstDisplayIndex.Count)
                    _ablnRendered = new bool[_lstDisplayIndex.Count];
                else
                {
                    for (int i = 0; i < _ablnRendered.Length; ++i)
                    {
                        _ablnRendered[i] &= _lstDisplayIndex[i] == aintOldDisplayIndex[i];
                    }
                }
            }
            finally
            {
                if (aintSharedOldDisplayIndexes != null)
                    ArrayPool<int>.Shared.Return(aintSharedOldDisplayIndexes);
            }
        }

        private void LoadScreenContent()
        {
            if (_lstContentList.Count == 0 || ListItemControlHeight == 0)
                return;

            int toload = _blnLoadVisibleOnly
                ? NumVisibleElements
                : _lstContentList.Count;

            int top = VerticalScroll.Value / ListItemControlHeight;

            LoadRange(top, top + toload);
        }

        private int NumVisibleElements
        {
            get
            {
                if (_lstContentList.Count == 0 || ListItemControlHeight == 0)
                    return 0;
                return Math.Min(this.DoThreadSafeFunc(x => x.Height) / ListItemControlHeight + 2, _lstContentList.Count);
            }
        }

        private void ResetDisplayPanelHeight(int intNumVisible = -1)
        {
            int intMyHeight = this.DoThreadSafeFunc(x => x.Height);
            pnlDisplay.DoThreadSafe(x => x.Height = Math.Max(intMyHeight,
                                                             (intNumVisible >= 0 ? intNumVisible : _lstContentList.Count(y => y.Visible)) * ListItemControlHeight));
        }

        private void RedrawControls(IEnumerable<ControlWithMetaData> lstToClear)
        {
            _blnAllRendered = false;
            int intNumVisible = _lstContentList.Count(x => x.Visible);
            foreach (ControlWithMetaData item in lstToClear)
            {
                if (item.Visible)
                    --intNumVisible;
                item.RefreshVisible();
                if (item.Visible)
                    ++intNumVisible;
            }
            ResetDisplayPanelHeight(intNumVisible);
            ComputeDisplayIndex();
            LoadScreenContent();
        }

        private void ApplicationOnIdle(object sender, EventArgs eventArgs)
        {
            if (_blnAllRendered)
                return;
            int firstUnrendered = _ablnRendered.FirstMatching(false);
            if (firstUnrendered == -1)
            {
                _blnAllRendered = true;
                return;
            }

            int end = _ablnRendered.FirstMatching(true, firstUnrendered);
            if (end == -1)
                end = _lstDisplayIndex.Count;

            end = Math.Min(end, firstUnrendered + _intOffScreenChunkSize);
            using (new FetchSafelyFromPool<Stopwatch>(Utils.StopwatchPool, out Stopwatch sw))
            {
                sw.Start();

                if (Interlocked.Increment(ref _intSuspendLayoutCount) == 1)
                    pnlDisplay.SuspendLayout();
                try
                {
                    LoadRange(firstUnrendered, end);
                }
                finally
                {
                    if (Interlocked.Decrement(ref _intSuspendLayoutCount) == 0)
                        pnlDisplay.ResumeLayout();
                }

                sw.Stop();

                if (sw.Elapsed > TimeSpan.FromSeconds(0.1f))
                {
                    if (_intOffScreenChunkSize > 1)
                    {
                        _intOffScreenChunkSize /= 2;
                        Log.Trace("Offscreen chunk render size decreased to " +
                                  _intOffScreenChunkSize.ToString(GlobalSettings.InvariantCultureInfo));
                    }
                }
                else if (sw.Elapsed < TimeSpan.FromSeconds(0.05f))
                {
                    _intOffScreenChunkSize *= 2;
                    Log.Trace("Offscreen chunk render size increased to " +
                              _intOffScreenChunkSize.ToString(GlobalSettings.InvariantCultureInfo));
                }
            }
        }

        public void Filter(Predicate<TType> predicate, bool forceRefresh = false)
        {
            if (_visibleFilter == predicate && !forceRefresh) return;
            _visibleFilter = predicate;

            if (Interlocked.Increment(ref _intSuspendLayoutCount) == 1)
                pnlDisplay.DoThreadSafe(x => x.SuspendLayout());
            try
            {
                RedrawControls(_lstContentList);
            }
            finally
            {
                if (Interlocked.Decrement(ref _intSuspendLayoutCount) == 0)
                    pnlDisplay.DoThreadSafe(x => x.ResumeLayout());
            }
        }

        public void Sort(IComparer<TType> comparison)
        {
            if (Equals(_comparison, comparison)) return;
            _comparison = comparison;

            if (Interlocked.Increment(ref _intSuspendLayoutCount) == 1)
                pnlDisplay.DoThreadSafe(x => x.SuspendLayout());
            try
            {
                RedrawControls(_lstContentList);
            }
            finally
            {
                if (Interlocked.Decrement(ref _intSuspendLayoutCount) == 0)
                    pnlDisplay.DoThreadSafe(x => x.ResumeLayout());
            }
        }

        private async Task ContentsChanged(object sender, ListChangedEventArgs eventArgs, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intNewIndex = eventArgs?.NewIndex ?? 0;
            IEnumerable<ControlWithMetaData> lstToRedraw;
            switch (eventArgs?.ListChangedType)
            {
                case ListChangedType.ItemChanged:
                    return;

                case ListChangedType.Reset:
                    if (Interlocked.Increment(ref _intSuspendLayoutCount) == 1)
                        await pnlDisplay.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                    try
                    {
                        foreach (ControlWithMetaData objLoopControl in _lstContentList)
                        {
                            await objLoopControl.CleanupAsync(token).ConfigureAwait(false);
                        }
                        _lstContentList.Clear();
                        foreach (TType objLoopTType in Contents)
                        {
                            _lstContentList.Add(await this.DoThreadSafeFuncAsync(x => new ControlWithMetaData(objLoopTType, x, false), token).ConfigureAwait(false));
                        }
                        await pnlDisplay.DoThreadSafeAsync(y => y.Controls.AddRange(_lstContentList.Select(x => x.Control).ToArray()), token).ConfigureAwait(false);
                    }
                    finally
                    {
                        if (Interlocked.Decrement(ref _intSuspendLayoutCount) == 0)
                            await pnlDisplay.DoThreadSafeAsync(x => x.ResumeLayout(), token).ConfigureAwait(false);
                    }
                    _indexComparer.Reset(Contents);
                    lstToRedraw = _lstContentList;
                    break;

                case ListChangedType.ItemAdded:
                    _lstContentList.Insert(intNewIndex, await this.DoThreadSafeFuncAsync(x => new ControlWithMetaData(x.Contents[intNewIndex], x), token).ConfigureAwait(false));
                    _indexComparer.Reset(Contents);
                    lstToRedraw = intNewIndex == 0 ? _lstContentList : _lstContentList.Skip(intNewIndex);
                    break;

                case ListChangedType.ItemDeleted:
                    await _lstContentList[intNewIndex].CleanupAsync(token).ConfigureAwait(false);
                    _lstContentList.RemoveAt(intNewIndex);
                    _indexComparer.Reset(Contents);
                    lstToRedraw = intNewIndex == 0 ? _lstContentList : _lstContentList.Skip(intNewIndex);
                    break;

                case ListChangedType.ItemMoved:
                    // Refresh the underlying lists, but do not refresh any displays
                    int intOldIndex = eventArgs.OldIndex;
                    int intDirection = intOldIndex < intNewIndex ? 1 : -1;
                    ControlWithMetaData objMovedControl = _lstContentList[intOldIndex];
                    int intLoopDisplayIndex = _lstDisplayIndex.IndexOf(intOldIndex);
                    int intFinalDisplayIndexValue = intLoopDisplayIndex >= 0 ? intNewIndex : -1;
                    bool blnFinalRenderedValue = intLoopDisplayIndex >= 0 && _ablnRendered[intLoopDisplayIndex];
                    for (int i = intOldIndex; i * intDirection < intNewIndex * intDirection; i += intDirection)
                    {
                        _lstContentList[i] = _lstContentList[i + intDirection];
                        int intDisplayIndex = _lstDisplayIndex.IndexOf(i + intDirection);
                        if (intDisplayIndex != -1)
                        {
                            if (intLoopDisplayIndex != -1)
                            {
                                _lstDisplayIndex[intLoopDisplayIndex] = _lstDisplayIndex[intDisplayIndex];
                                _ablnRendered[intLoopDisplayIndex] = _ablnRendered[intDisplayIndex];
                            }
                            else
                            {
                                intFinalDisplayIndexValue = i;
                                blnFinalRenderedValue = _ablnRendered[intDisplayIndex];
                            }
                            intLoopDisplayIndex = intDisplayIndex;
                        }
                    }
                    _lstContentList[intNewIndex] = objMovedControl;
                    if (intLoopDisplayIndex != -1)
                    {
                        _lstDisplayIndex[intLoopDisplayIndex] = intFinalDisplayIndexValue;
                        _ablnRendered[intLoopDisplayIndex] = blnFinalRenderedValue;
                    }
                    return;
                //case ListChangedType.PropertyDescriptorAdded:
                //    break;
                //case ListChangedType.PropertyDescriptorDeleted:
                //    break;
                //case ListChangedType.PropertyDescriptorChanged:
                //    break;
                default:
                    Utils.BreakIfDebug();
                    return;
            }
            if (lstToRedraw != null)
            {
                if (Interlocked.Increment(ref _intSuspendLayoutCount) == 1)
                    await pnlDisplay.DoThreadSafeAsync(x => x.SuspendLayout(), token).ConfigureAwait(false);
                try
                {
                    if (ChildPropertyChangedAsync != null)
                        await ChildPropertyChangedAsync.Invoke(this, new PropertyChangedEventArgs(nameof(Contents)), token).ConfigureAwait(false);
                    ChildPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Contents)));
                    await this.DoThreadSafeAsync(x => x.RedrawControls(lstToRedraw), token).ConfigureAwait(false);
                }
                finally
                {
                    if (Interlocked.Decrement(ref _intSuspendLayoutCount) == 0)
                        await pnlDisplay.DoThreadSafeAsync(x => x.ResumeLayout(), token).ConfigureAwait(false);
                }
            }
        }

        private void BindingListDisplay_Scroll(object sender, ScrollEventArgs e)
        {
            if (Interlocked.Increment(ref _intSuspendLayoutCount) == 1)
                pnlDisplay.SuspendLayout();
            try
            {
                LoadScreenContent();
            }
            finally
            {
                if (Interlocked.Decrement(ref _intSuspendLayoutCount) == 0)
                    pnlDisplay.ResumeLayout();
            }
        }

        private void BindingListDisplay_SizeChanged(object sender, EventArgs e)
        {
            if (Interlocked.Increment(ref _intSuspendLayoutCount) == 1)
                pnlDisplay.SuspendLayout();
            try
            {
                pnlDisplay.Width = Width - SystemInformation.VerticalScrollBarWidth;
                ResetDisplayPanelHeight();
                foreach (Control control in pnlDisplay.Controls)
                {
                    if (control.AutoSize)
                    {
                        if (control.MinimumSize.Width > pnlDisplay.Width)
                            control.MinimumSize = new Size(pnlDisplay.Width, control.MinimumSize.Height);
                        if (control.MaximumSize.Width != pnlDisplay.Width)
                            control.MaximumSize = new Size(pnlDisplay.Width, control.MaximumSize.Height);
                        if (control.MinimumSize.Width < pnlDisplay.Width)
                            control.MinimumSize = new Size(pnlDisplay.Width, control.MinimumSize.Height);
                    }
                    else
                    {
                        control.Width = pnlDisplay.Width;
                    }
                }
                // Needed for safety reasons in case control size is decreased horizontally
                pnlDisplay.Width = Width - SystemInformation.VerticalScrollBarWidth;
            }
            finally
            {
                if (Interlocked.Decrement(ref _intSuspendLayoutCount) == 0)
                    pnlDisplay.ResumeLayout();
            }
        }

        private sealed class ControlWithMetaData : IDisposable
        {
            public TType Item { get; }

            public Control Control
            {
                get
                {
                    if (_objControl == null)
                        CreateControl();
                    return _objControl;
                }
            }

            public bool Visible => _visible ?? (_visible = _parent._visibleFilter(Item)).Value;

            private readonly BindingListDisplay<TType> _parent;
            private Control _objControl;
            private bool? _visible;

            public ControlWithMetaData(TType item, BindingListDisplay<TType> parent, bool blnAddControlAfterCreation = true)
            {
                _parent = parent;
                Item = item;
                // Because binding list displays generally involve syncing the name label of child controls after-the-fact,
                // we need to create the control in the constructor (even if it isn't rendered) so that we can measure its
                // elements' widths and/or heights
                CreateControl(blnAddControlAfterCreation);
                switch (item)
                {
                    case INotifyPropertyChangedAsync prop when prop is IHasLockObject objHasLock:
                        try
                        {
                            using (objHasLock.LockObject.EnterWriteLock())
                                prop.PropertyChangedAsync += item_ChangedEventAsync;
                        }
                        catch (ObjectDisposedException)
                        {
                            // swallow this
                        }

                        break;
                    case INotifyPropertyChangedAsync prop:
                        prop.PropertyChangedAsync += item_ChangedEventAsync;
                        break;
                    case INotifyPropertyChanged prop2 when prop2 is IHasLockObject objHasLock:
                        try
                        {
                            using (objHasLock.LockObject.EnterWriteLock())
                                prop2.PropertyChanged += item_ChangedEvent;
                        }
                        catch (ObjectDisposedException)
                        {
                            // swallow this
                        }

                        break;
                    case INotifyPropertyChanged prop2:
                        prop2.PropertyChanged += item_ChangedEvent;
                        break;
                }
            }

            private void item_ChangedEvent(object sender, PropertyChangedEventArgs e)
            {
                bool changes = false;
                if (_visible != null && _visible.Value != _parent._visibleFilter(Item))
                {
                    changes = true;
                    _visible = !_visible;
                }
                //TODO: Add this back in, but it is spamming updates like crazy right now and not updating right
                //else if (_visible != null && _visible.Value)
                //{
                //    int displayIndex = _parent._displayIndex.FindIndex(x => _parent._contentList[x] == this);
                //    if (displayIndex > 0)
                //    {
                //        if(_parent._comparison.Compare(Item, _parent._contentList[_parent._displayIndex[displayIndex - 1]].Item) > 0)
                //        {
                //            changes = true;
                //        }
                //    }
                //    if(_parent._displayIndex.Count - 1 > displayIndex)
                //    {
                //        if (_parent._comparison.Compare(Item, _parent._contentList[_parent._displayIndex[displayIndex + 1]].Item) < 0)
                //        {
                //            changes = true;
                //        }
                //    }
                //}

                _parent.ChildPropertyChanged?.Invoke(sender, e);
                if (changes)
                {
                    _parent.RedrawControls(this.Yield());
                }
            }

            private async Task item_ChangedEventAsync(object sender, PropertyChangedEventArgs e, CancellationToken token = default)
            {
                bool changes = false;
                if (_visible != null && _visible.Value != await _parent.DoThreadSafeFuncAsync(x => x._visibleFilter(Item), token).ConfigureAwait(false))
                {
                    changes = true;
                    _visible = !_visible;
                }
                //TODO: Add this back in, but it is spamming updates like crazy right now and not updating right
                //else if (_visible != null && _visible.Value)
                //{
                //    int displayIndex = _parent._displayIndex.FindIndex(x => _parent._contentList[x] == this);
                //    if (displayIndex > 0)
                //    {
                //        if(_parent._comparison.Compare(Item, _parent._contentList[_parent._displayIndex[displayIndex - 1]].Item) > 0)
                //        {
                //            changes = true;
                //        }
                //    }
                //    if(_parent._displayIndex.Count - 1 > displayIndex)
                //    {
                //        if (_parent._comparison.Compare(Item, _parent._contentList[_parent._displayIndex[displayIndex + 1]].Item) < 0)
                //        {
                //            changes = true;
                //        }
                //    }
                //}

                if (_parent.ChildPropertyChangedAsync != null)
                    await _parent.ChildPropertyChangedAsync.Invoke(sender, e, token).ConfigureAwait(false);
                if (changes)
                {
                    await _parent.DoThreadSafeAsync(x => x.RedrawControls(this.Yield()), token).ConfigureAwait(false);
                }
            }

            /// <summary>
            /// Updates the height of the contained control if it exists. Necessary to ensure that the contained control doesn't get created prematurely.
            /// </summary>
            public void UpdateHeight()
            {
                _objControl?.DoThreadSafe(x =>
                {
                    int intParentControlHeight = _parent.ListItemControlHeight;
                    if (x.AutoSize)
                    {
                        if (x.MinimumSize.Height > intParentControlHeight)
                            x.MinimumSize = new Size(x.MinimumSize.Width, intParentControlHeight);
                        if (x.MaximumSize.Height != intParentControlHeight)
                            x.MaximumSize = new Size(x.MaximumSize.Width, intParentControlHeight);
                        if (x.MinimumSize.Height < intParentControlHeight)
                            x.MinimumSize = new Size(x.MinimumSize.Width, intParentControlHeight);
                    }
                    else
                    {
                        x.Height = intParentControlHeight;
                    }
                });
            }

            private void CreateControl(bool blnAddControlAfterCreation = true)
            {
                Control objNewControl = _parent.DoThreadSafeFunc(x => x._funcCreateControl(Item));
                if (Interlocked.CompareExchange(ref _objControl, objNewControl, null) != null)
                {
                    Reset();
                    objNewControl.Dispose();
                    return;
                }
                _objControl = _parent.DoThreadSafeFunc(x => x._funcCreateControl(Item));
                objNewControl.DoThreadSafe(x =>
                {
                    x.SuspendLayout();
                    try
                    {
                        x.Visible = false;
                        _parent.ListItemControlHeight = Math.Max(_parent.ListItemControlHeight, x.PreferredSize.Height);
                        if (x.AutoSize)
                        {
                            x.MinimumSize = new Size(_parent.DisplayPanel.DoThreadSafeFunc(y => y.Width), _parent.ListItemControlHeight);
                            x.MaximumSize = new Size(_parent.DisplayPanel.DoThreadSafeFunc(y => y.Width), _parent.ListItemControlHeight);
                        }
                        else
                        {
                            x.Width = _parent.DisplayPanel.DoThreadSafeFunc(y => y.Width);
                            x.Height = _parent.ListItemControlHeight;
                        }
                    }
                    finally
                    {
                        x.ResumeLayout();
                    }
                });
                if (blnAddControlAfterCreation)
                    _parent.DisplayPanel.DoThreadSafe(x => x.Controls.Add(objNewControl));
            }

            public void RefreshVisible()
            {
                _visible = _parent.DoThreadSafeFunc(x => x._visibleFilter(Item));
                if (!_visible.Value)
                    _objControl?.DoThreadSafe(x => x.Visible = false);
            }

            public void Reset()
            {
                _visible = null;
                _objControl?.DoThreadSafe(x =>
                {
                    x.Visible = false;
                    x.Location = new Point(0, 0);
                    if (x.AutoSize)
                    {
                        x.MinimumSize = new Size(_parent.DisplayPanel.DoThreadSafeFunc(y => y.Width), _parent.ListItemControlHeight);
                        x.MaximumSize = new Size(_parent.DisplayPanel.DoThreadSafeFunc(y => y.Width), _parent.ListItemControlHeight);
                    }
                    else
                    {
                        x.Width = _parent.DisplayPanel.DoThreadSafeFunc(y => y.Width);
                        x.Height = _parent.ListItemControlHeight;
                    }
                });
            }

            public void Cleanup()
            {
                if (_objControl == null)
                    return;
                switch (Item)
                {
                    case INotifyPropertyChangedAsync prop when prop is IHasLockObject objHasLock:
                        try
                        {
                            using (objHasLock.LockObject.EnterWriteLock())
                                prop.PropertyChangedAsync -= item_ChangedEventAsync;
                        }
                        catch (ObjectDisposedException)
                        {
                            // swallow this
                        }

                        break;
                    case INotifyPropertyChangedAsync prop:
                        prop.PropertyChangedAsync -= item_ChangedEventAsync;
                        break;
                    case INotifyPropertyChanged prop2 when prop2 is IHasLockObject objHasLock:
                        try
                        {
                            using (objHasLock.LockObject.EnterWriteLock())
                                prop2.PropertyChanged -= item_ChangedEvent;
                        }
                        catch (ObjectDisposedException)
                        {
                            // swallow this
                        }

                        break;
                    case INotifyPropertyChanged prop2:
                        prop2.PropertyChanged -= item_ChangedEvent;
                        break;
                }
                _parent.DisplayPanel.DoThreadSafe(x => x.Controls.Remove(Control));
                Interlocked.Exchange(ref _objControl, null)?.DoThreadSafe(x => x.Dispose());
            }

            public async Task CleanupAsync(CancellationToken token = default)
            {
                if (_objControl == null)
                    return;

                switch (Item)
                {
                    case INotifyPropertyChangedAsync prop when prop is IHasLockObject objHasLock:
                        try
                        {
                            IAsyncDisposable objLocker = await objHasLock.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                            try
                            {
                                prop.PropertyChangedAsync -= item_ChangedEventAsync;
                            }
                            finally
                            {
                                await objLocker.DisposeAsync().ConfigureAwait(false);
                            }
                        }
                        catch (ObjectDisposedException)
                        {
                            // swallow this
                        }

                        break;
                    case INotifyPropertyChangedAsync prop:
                        prop.PropertyChangedAsync -= item_ChangedEventAsync;
                        break;
                    case INotifyPropertyChanged prop2 when prop2 is IHasLockObject objHasLock:
                        try
                        {
                            IAsyncDisposable objLocker = await objHasLock.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                            try
                            {
                                prop2.PropertyChanged -= item_ChangedEvent;
                            }
                            finally
                            {
                                await objLocker.DisposeAsync().ConfigureAwait(false);
                            }
                        }
                        catch (ObjectDisposedException)
                        {
                            // swallow this
                        }

                        break;
                    case INotifyPropertyChanged prop2:
                        prop2.PropertyChanged -= item_ChangedEvent;
                        break;
                }

                await _parent.DisplayPanel.DoThreadSafeAsync(x => x.Controls.Remove(Control), token).ConfigureAwait(false);
                Control objControl = Interlocked.Exchange(ref _objControl, null);
                if (objControl != null)
                    await objControl.DoThreadSafeAsync(x => x.Dispose(), token).ConfigureAwait(false);
            }

            public void Dispose()
            {
                Interlocked.Exchange(ref _objControl, null)?.DoThreadSafe(x => x.Dispose());
            }
        }

        private sealed class IndexComparer : IComparer<TType>
        {
            private readonly Dictionary<TType, int> _dicIndeces = new Dictionary<TType, int>();

            public int Compare(TType x, TType y)
            {
                if (x != null && _dicIndeces.TryGetValue(x, out int xindex))
                {
                    if (y != null && _dicIndeces.TryGetValue(y, out int yindex))
                    {
                        return xindex.CompareTo(yindex);
                    }

                    Utils.BreakIfDebug();
                    return 1;
                }

                Utils.BreakIfDebug();
                if (y != null && (x == null || _dicIndeces.ContainsKey(y)))
                    return -1;

                return 0;
            }

            public IndexComparer(IReadOnlyList<TType> list)
            {
                Reset(list);
            }

            public void Reset(IReadOnlyList<TType> source)
            {
                _dicIndeces.Clear();
                for (int i = 0; i < source.Count; i++)
                {
                    _dicIndeces.Add(source[i], i);
                }
            }
        }

        private void BindingListDisplay_DpiChangedAfterParent(object sender, EventArgs e)
        {
            int intMaxControlHeight = 0;
            foreach (ControlWithMetaData objControl in _lstContentList)
            {
                intMaxControlHeight = Math.Max(objControl.Control.PreferredSize.Height, intMaxControlHeight);
            }
            ListItemControlHeight = intMaxControlHeight;
        }
    }
}
