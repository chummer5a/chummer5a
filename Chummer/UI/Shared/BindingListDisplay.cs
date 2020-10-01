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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NLog;

namespace Chummer.UI.Shared
{
    public partial class BindingListDisplay<TType> : UserControl
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BindingListDisplay<TType>));
        public PropertyChangedEventHandler ChildPropertyChanged { get; set; }

        private bool _blnIsTopmostSuspendLayout;
        private readonly Func<TType, Control> _funcCreateControl;  //Function to create a control out of a item
        private readonly bool _blnLoadVisibleOnly;
        private readonly List<ControlWithMetaData> _lstContentList = new List<ControlWithMetaData>(10);
        private readonly List<int> _lstDisplayIndex = new List<int>(10);
        private readonly IndexComparer _indexComparer;
        private BitArray _ablnRendered;
        private int _intOffScreenChunkSize = 1;
        private int _intListItemControlHeight;
        private bool _blnAllRendered;
        private Predicate<TType> _visibleFilter = x => true;
        private IComparer<TType> _comparison;

        public BindingListDisplay(BindingList<TType> contents, Func<TType, Control> funcCreateControl, bool blnLoadVisibleOnly = true)
        {
            InitializeComponent();
            Contents = contents ?? throw new ArgumentNullException(nameof(contents));
            _funcCreateControl = funcCreateControl;
            _blnLoadVisibleOnly = blnLoadVisibleOnly;
            DisplayPanel.SuspendLayout();
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

                DisplayPanel.Controls.AddRange(_lstContentList.Select(x => x.Control).ToArray());
                _indexComparer = new IndexComparer(Contents);
                _comparison = _comparison ?? _indexComparer;
                Contents.ListChanged += ContentsChanged;
                ComptuteDisplayIndex();
                LoadScreenContent();
                BindingListDisplay_SizeChanged(null, null);
            }
            finally
            {
                _blnIsTopmostSuspendLayout = true;
                DisplayPanel.ResumeLayout();
            }
        }

        private void BindingListDisplay_Load(object sender, EventArgs e)
        {
            Application.Idle += ApplicationOnIdle;
        }

        /// <summary>
        /// Base BindingList that represents all possible contents of the display, not necessarily all visible.
        /// </summary>
        public BindingList<TType> Contents { get; }

        public Panel DisplayPanel => pnlDisplay;

        private int ListItemControlHeight
        {
            get => _intListItemControlHeight;
            set
            {
                if (_intListItemControlHeight != value)
                {
                    _intListItemControlHeight = value;
                    foreach (ControlWithMetaData objControl in _lstContentList)
                    {
                        objControl.UpdateHeight();
                    }
                }
            }
        }

        private void LoadRange(int min, int max)
        {
            min = Math.Max(0, min);
            max = Math.Min(_lstDisplayIndex.Count, max);
            if (_ablnRendered.FirstMatching(false, min) > max)
                return;
            bool blnIsTopmostSuspendLayout = _blnIsTopmostSuspendLayout;
            if (blnIsTopmostSuspendLayout)
                _blnIsTopmostSuspendLayout = false;
            try
            {
                DisplayPanel.SuspendLayout();
                for (int i = min; i < max; ++i)
                {
                    if (_ablnRendered[i])
                        continue;

                    ControlWithMetaData item = _lstContentList[_lstDisplayIndex[i]];
                    item.Control.Location = new Point(0, i * ListItemControlHeight);
                    item.Control.Visible = true;
                    _ablnRendered[i] = true;
                }
            }
            finally
            {
                if (blnIsTopmostSuspendLayout)
                {
                    _blnIsTopmostSuspendLayout = true;
                    DisplayPanel.ResumeLayout();
                }
            }
        }

        private void ComptuteDisplayIndex()
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
            Span<int> aintOldDisplayIndex = _lstDisplayIndex.Count > GlobalOptions.MaxStackLimit
                ? new int[_lstDisplayIndex.Count]
                : stackalloc int[_lstDisplayIndex.Count];
            for (int i = 0; i < aintOldDisplayIndex.Length; ++i)
                aintOldDisplayIndex[i] = _lstDisplayIndex[i];
            _lstDisplayIndex.Clear();
            _lstDisplayIndex.AddRange(objTTypeList.Select(x => x.Item2));

            if (_ablnRendered == null || _ablnRendered.Length != _lstDisplayIndex.Count)
                _ablnRendered = new BitArray(_lstDisplayIndex.Count);
            else
            {
                for (int i = 0; i < _ablnRendered.Count; ++i)
                {
                    _ablnRendered[i] = _ablnRendered[i] && _lstDisplayIndex[i] == aintOldDisplayIndex[i];
                }
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
                return Math.Min(Height / ListItemControlHeight + 2, _lstContentList.Count);
            }
        }

        private void ResetDisplayPanelHeight(int intNumVisible = -1)
        {
            DisplayPanel.Height = Math.Max(Height, (intNumVisible >= 0 ? intNumVisible : _lstContentList.Count(x => x.Visible)) * ListItemControlHeight);
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
            ComptuteDisplayIndex();
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
            Stopwatch sw = Stopwatch.StartNew();

            bool blnIsTopmostSuspendLayout = _blnIsTopmostSuspendLayout;
            if (blnIsTopmostSuspendLayout)
                _blnIsTopmostSuspendLayout = false;
            try
            {
                DisplayPanel.SuspendLayout();
                LoadRange(firstUnrendered, end);
            }
            finally
            {
                if (blnIsTopmostSuspendLayout)
                {
                    _blnIsTopmostSuspendLayout = true;
                    DisplayPanel.ResumeLayout();
                }
            }

            sw.Stop();

            if (sw.Elapsed > TimeSpan.FromSeconds(0.1f))
            {
                if (_intOffScreenChunkSize > 1)
                {
                    _intOffScreenChunkSize /= 2;
                    Log.Info("Offscreen chunk render size decreased to " + _intOffScreenChunkSize.ToString(GlobalOptions.InvariantCultureInfo));
                }
            }
            else if (sw.Elapsed < TimeSpan.FromSeconds(0.05f))
            {
                _intOffScreenChunkSize *= 2;
                Log.Info("Offscreen chunk render size increased to " + _intOffScreenChunkSize.ToString(GlobalOptions.InvariantCultureInfo));
            }
        }

        public void Filter(Predicate<TType> predicate, bool forceRefresh = false)
        {
            if (_visibleFilter == predicate && !forceRefresh) return;
            _visibleFilter = predicate;

            bool blnIsTopmostSuspendLayout = _blnIsTopmostSuspendLayout;
            if (blnIsTopmostSuspendLayout)
                _blnIsTopmostSuspendLayout = false;
            try
            {
                DisplayPanel.SuspendLayout();
                RedrawControls(_lstContentList);
            }
            finally
            {
                if (blnIsTopmostSuspendLayout)
                {
                    _blnIsTopmostSuspendLayout = true;
                    DisplayPanel.ResumeLayout();
                }
            }
        }

        public void Sort(IComparer<TType> comparison)
        {
            if (Equals(_comparison, comparison)) return;
            _comparison = comparison;

            bool blnIsTopmostSuspendLayout = _blnIsTopmostSuspendLayout;
            if (blnIsTopmostSuspendLayout)
                _blnIsTopmostSuspendLayout = false;
            try
            {
                DisplayPanel.SuspendLayout();
                RedrawControls(_lstContentList);
            }
            finally
            {
                if (blnIsTopmostSuspendLayout)
                {
                    _blnIsTopmostSuspendLayout = true;
                    DisplayPanel.ResumeLayout();
                }
            }
        }

        private void ContentsChanged(object sender, ListChangedEventArgs eventArgs)
        {
            int intNewIndex = eventArgs?.NewIndex ?? 0;
            IEnumerable<ControlWithMetaData> lstToRedraw;
            switch (eventArgs?.ListChangedType)
            {
                case ListChangedType.ItemChanged:
                    return;
                case ListChangedType.Reset:
                    bool blnIsTopmostSuspendLayout = _blnIsTopmostSuspendLayout;
                    if (blnIsTopmostSuspendLayout)
                        _blnIsTopmostSuspendLayout = false;
                    try
                    {
                        DisplayPanel.SuspendLayout();
                        foreach (ControlWithMetaData objLoopControl in _lstContentList)
                        {
                            objLoopControl.Cleanup();
                        }
                        _lstContentList.Clear();
                        foreach (TType objLoopTType in Contents)
                        {
                            _lstContentList.Add(new ControlWithMetaData(objLoopTType, this, false));
                        }
                        DisplayPanel.Controls.AddRange(_lstContentList.Select(x => x.Control).ToArray());
                    }
                    finally
                    {
                        if (blnIsTopmostSuspendLayout)
                        {
                            _blnIsTopmostSuspendLayout = true;
                            DisplayPanel.ResumeLayout();
                        }
                    }
                    _indexComparer.Reset(Contents);
                    lstToRedraw = _lstContentList;
                    break;
                case ListChangedType.ItemAdded:
                    _lstContentList.Insert(intNewIndex, new ControlWithMetaData(Contents[intNewIndex], this));
                    _indexComparer.Reset(Contents);
                    lstToRedraw = _lstContentList.Skip(intNewIndex);
                    break;
                case ListChangedType.ItemDeleted:
                    _lstContentList[intNewIndex].Cleanup();
                    _lstContentList.RemoveAt(intNewIndex);
                    _indexComparer.Reset(Contents);
                    lstToRedraw = _lstContentList.Skip(intNewIndex);
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
                bool blnIsTopmostSuspendLayout = _blnIsTopmostSuspendLayout;
                if (blnIsTopmostSuspendLayout)
                    _blnIsTopmostSuspendLayout = false;
                try
                {
                    DisplayPanel.SuspendLayout();
                    ChildPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Contents)));
                    RedrawControls(lstToRedraw);
                }
                finally
                {
                    if (blnIsTopmostSuspendLayout)
                    {
                        _blnIsTopmostSuspendLayout = true;
                        DisplayPanel.ResumeLayout();
                    }
                }
            }
        }

        private void BindingListDisplay_Scroll(object sender, ScrollEventArgs e)
        {
            bool blnIsTopmostSuspendLayout = _blnIsTopmostSuspendLayout;
            if (blnIsTopmostSuspendLayout)
                _blnIsTopmostSuspendLayout = false;
            try
            {
                DisplayPanel.SuspendLayout();
                LoadScreenContent();
            }
            finally
            {
                if (blnIsTopmostSuspendLayout)
                {
                    _blnIsTopmostSuspendLayout = true;
                    DisplayPanel.ResumeLayout();
                }
            }
        }

        private void BindingListDisplay_SizeChanged(object sender, EventArgs e)
        {
            bool blnIsTopmostSuspendLayout = _blnIsTopmostSuspendLayout;
            if (blnIsTopmostSuspendLayout)
                _blnIsTopmostSuspendLayout = false;
            try
            {
                DisplayPanel.SuspendLayout();
                DisplayPanel.Width = Width - SystemInformation.VerticalScrollBarWidth;
                ResetDisplayPanelHeight();
                foreach (Control control in DisplayPanel.Controls)
                {
                    if (control.AutoSize)
                    {
                        if (control.MinimumSize.Width > DisplayPanel.Width)
                            control.MinimumSize = new Size(DisplayPanel.Width, control.MinimumSize.Height);
                        if (control.MaximumSize.Width != DisplayPanel.Width)
                            control.MaximumSize = new Size(DisplayPanel.Width, control.MaximumSize.Height);
                        if (control.MinimumSize.Width < DisplayPanel.Width)
                            control.MinimumSize = new Size(DisplayPanel.Width, control.MinimumSize.Height);
                    }
                    else
                    {
                        control.Width = DisplayPanel.Width;
                    }
                }
                // Needed for safety reasons in case control size is decreased horizontally
                DisplayPanel.Width = Width - SystemInformation.VerticalScrollBarWidth;
            }
            finally
            {
                if (blnIsTopmostSuspendLayout)
                {
                    _blnIsTopmostSuspendLayout = true;
                    DisplayPanel.ResumeLayout();
                }
            }
        }

        private sealed class ControlWithMetaData
        {
            public TType Item { get; }

            public Control Control
            {
                get
                {
                    if (_control == null)
                        CreateControl();
                    return _control;
                }
            }

            public bool Visible
            {
                get
                {
                    if (_visible == null)
                        _visible = _parent._visibleFilter(Item);

                    return _visible.Value;
                }
            }

            private readonly BindingListDisplay<TType> _parent;
            private Control _control;
            private bool? _visible;

            public ControlWithMetaData(TType item, BindingListDisplay<TType> parent, bool blnAddControlAfterCreation = true)
            {
                _parent = parent;
                Item = item;
                // Because binding list displays generally involve syncing the name label of child controls after-the-fact,
                // we need to create the control in the constructor (even if it isn't rendered) so that we can measure its
                // elements' widths and/or heights
                CreateControl(blnAddControlAfterCreation);
                if (item is INotifyPropertyChanged prop)
                {
                    prop.PropertyChanged += item_ChangedEvent;
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

            /// <summary>
            /// Updates the height of the contained control if it exists. Necessary to ensure that the contained control doesn't get created prematurely.
            /// </summary>
            public void UpdateHeight()
            {
                if (_control != null)
                {
                    if (_control.AutoSize)
                    {
                        if (_control.MinimumSize.Height > _parent.ListItemControlHeight)
                            _control.MinimumSize = new Size(_control.MinimumSize.Width, _parent.ListItemControlHeight);
                        if (_control.MaximumSize.Height != _parent.ListItemControlHeight)
                            _control.MaximumSize = new Size(_control.MaximumSize.Width, _parent.ListItemControlHeight);
                        if (_control.MinimumSize.Height < _parent.ListItemControlHeight)
                            _control.MinimumSize = new Size(_control.MinimumSize.Width, _parent.ListItemControlHeight);
                    }
                    else
                    {
                        _control.Height = _parent.ListItemControlHeight;
                    }
                }
            }

            private void CreateControl(bool blnAddControlAfterCreation = true)
            {
                _control = _parent._funcCreateControl(Item);
                _control.SuspendLayout();
                _control.Visible = false;
                if (_parent.ListItemControlHeight < _control.PreferredSize.Height)
                {
                    _parent.ListItemControlHeight = _control.PreferredSize.Height;
                }
                if (_control.AutoSize)
                {
                    _control.MinimumSize = new Size(_parent.DisplayPanel.Width, _parent.ListItemControlHeight);
                    _control.MaximumSize = new Size(_parent.DisplayPanel.Width, _parent.ListItemControlHeight);
                }
                else
                {
                    _control.Width = _parent.DisplayPanel.Width;
                    _control.Height = _parent.ListItemControlHeight;
                }
                _control.ResumeLayout();
                if (blnAddControlAfterCreation)
                    _parent.DisplayPanel.Controls.Add(_control);
            }

            public void RefreshVisible()
            {
                _visible = _parent._visibleFilter(Item);
                if (!_visible.Value && _control != null)
                {
                    _control.Visible = false;
                }
            }

            public void Reset()
            {
                _visible = null;
                if (_control != null)
                {
                    _control.Visible = false;
                    _control.Location = new Point(0, 0);
                    if (_control.AutoSize)
                    {
                        _control.MinimumSize = new Size(_parent.DisplayPanel.Width, _parent.ListItemControlHeight);
                        _control.MaximumSize = new Size(_parent.DisplayPanel.Width, _parent.ListItemControlHeight);
                    }
                    else
                    {
                        _control.Width = _parent.DisplayPanel.Width;
                        _control.Height = _parent.ListItemControlHeight;
                    }
                }
            }

            public void Cleanup()
            {
                if (_control != null)
                {
                    if (Item is INotifyPropertyChanged prop)
                    {
                        prop.PropertyChanged -= item_ChangedEvent;
                    }
                    _parent.DisplayPanel.Controls.Remove(Control);
                    _control.Dispose();
                    _control = null;
                }
            }
        }

        private sealed class IndexComparer : IComparer<TType>
        {
            private Dictionary<TType, int> _index;

            public int Compare(TType x, TType y)
            {
                if (x != null && _index.TryGetValue(x, out int xindex))
                {
                    if (y != null && _index.TryGetValue(y, out int yindex))
                    {
                        return xindex.CompareTo(yindex);
                    }

                    Utils.BreakIfDebug();
                    return 1;
                }

                Utils.BreakIfDebug();
                if (y != null && (x == null || _index.ContainsKey(y)))
                    return -1;

                return 0;
            }

            public IndexComparer(IReadOnlyList<TType> list)
            {
                Reset(list);
            }

            public void Reset(IReadOnlyList<TType> source)
            {
                _index = new Dictionary<TType, int>();
                for (int i = 0; i < source.Count; i++)
                {
                    _index.Add(source[i], i);
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
