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

        public IComparer<TType> DefaultComparer => _indexComparer;

        private readonly BindingList<TType> _contents; //List of all items supposed to be displayed
        private readonly Func<TType, Control> _createFunc;  //Function to create a control out of a item
        private readonly bool _loadVisibleOnly;
        private readonly List<ControlWithMetaData> _contentList;
        private readonly List<int> _displayIndex = new List<int>();
        private readonly IndexComparer _indexComparer;
        private BitArray _rendered;
        private int _offScreenChunkSize = 1;
        private bool _allRendered;
        private bool _resetAtIdle;
        private Predicate<TType> _visibleFilter = x => true;
        private IComparer<TType> _comparison;

        public BindingListDisplay(BindingList<TType> contents, Func<TType, Control> createFunc, bool loadVisibleOnly = true)
        {
            InitializeComponent();
            _contents = contents ?? throw new ArgumentNullException(nameof(contents));
            _createFunc = createFunc;
            _loadVisibleOnly = loadVisibleOnly;
            pnlDisplay.SuspendLayout();
            _contentList = new List<ControlWithMetaData>();
            foreach (TType objLoopTType in _contents)
            {
                _contentList.Add(new ControlWithMetaData(objLoopTType, this));
            }
            _indexComparer = new IndexComparer(_contents);
            _comparison = _comparison ?? _indexComparer;
            _contents.ListChanged += ContentsChanged;
            ComptuteDisplayIndex();
            LoadScreenContent();
            BindingListDisplay_SizeChanged(null, null);
            pnlDisplay.ResumeLayout();
        }

        private void BindingListDisplay_Load(object sender, EventArgs e)
        {
            DoubleBuffered = true;
            Application.Idle += ApplicationOnIdle;
        }

        /// <summary>
        /// Base BindingList that represents all possible contents of the display, not necessarily all visible.
        /// </summary>
        public BindingList<TType> Contents => _contents;

        private void LoadRange(int min, int max)
        {
            min = Math.Max(0, min);
            max = Math.Min(_displayIndex.Count, max);
            if (_rendered.FirstMatching(false, min) > max) return;
            SuspendLayout();
            for (int i = min; i < max; i++)
            {
                if (_rendered[i]) continue;

                ControlWithMetaData item = _contentList[_displayIndex[i]];

                item.Control.Location = new Point(0, i * _contentList[0].Control.Height);
                item.Control.Visible = true;
                _rendered[i] = true;
            }
            ResumeLayout();
        }

        private void ComptuteDisplayIndex()
        {
            Dictionary<TType, int> objTTypeIndexDictionary = new Dictionary<TType, int>();
            List<TType> objTTypeList = new List<TType>();
            for (int i = 0; i < _contentList.Count; i++)
            {
                ControlWithMetaData objLoopControl = _contentList[i];
                if (objLoopControl.Visible)
                {
                    objTTypeIndexDictionary.Add(objLoopControl.Item, i);
                    objTTypeList.Add(objLoopControl.Item);
                }
            }

            objTTypeList.Sort(_comparison);

            _displayIndex.Clear();
            foreach (TType objLoopTType in objTTypeList)
            {
                _displayIndex.Add(objTTypeIndexDictionary[objLoopTType]);
            }

            if (_rendered == null || _rendered.Length != _displayIndex.Count)
                _rendered = new BitArray(_displayIndex.Count);
            else
                _rendered.SetAll(false);
        }

        private void LoadScreenContent()
        {
            if (_contentList.Count == 0) return;

            int toload = _loadVisibleOnly
                ? VisibleElements()
                : _contentList.Count;

            int top = VerticalScroll.Value/_contentList[0].Control.Height;

            LoadRange(top, top + toload);
        }

        private int VisibleElements()
        {
            return _contentList.Count == 0 ? 0 : Math.Min(Height / _contentList[0].Control.Height + 2, _contentList.Count);
        }

        private void ClearCache(IEnumerable<ControlWithMetaData> lstToClear)
        {
            _allRendered = false;
            foreach (ControlWithMetaData item in lstToClear)
            {
                item.Reset();
            }
            pnlDisplay.Height = _contentList.Count == 0 ? Height : _contentList.Count(x => x.Visible) * _contentList[0].Control.Height;
            ComptuteDisplayIndex();
        }

        private void ApplicationOnIdle(object sender, EventArgs eventArgs)
        {
            TimeSpan maxDelay =TimeSpan.FromSeconds(0.1f);

            if (_resetAtIdle)
            {
                _resetAtIdle = false;
                pnlDisplay.SuspendLayout();
                ClearCache(_contentList);
                LoadScreenContent();  //TODO: Don't do this and call if becomes visible
                pnlDisplay.ResumeLayout();
            }

            if (_allRendered) return;
            int firstUnrendered = _rendered.FirstMatching(false);

            if (firstUnrendered == -1)
            {
                _allRendered = true;
                return;
            }

            int end = _rendered.FirstMatching(true, firstUnrendered);
            if (end == -1) end = _displayIndex.Count;

            end = Math.Min(end, firstUnrendered + _offScreenChunkSize);
            Stopwatch sw = Stopwatch.StartNew();

            pnlDisplay.SuspendLayout();
            LoadRange(firstUnrendered, end);
            pnlDisplay.ResumeLayout();

            sw.Stop();

            if (sw.Elapsed > maxDelay && _offScreenChunkSize > 1)
            {
                _offScreenChunkSize /= 2;
            }
            else if (maxDelay > sw.Elapsed)
            {
                _offScreenChunkSize++;
                Log.Info("Offscreen chunk render size increased to " + _offScreenChunkSize);
            }
        }

        public void Filter(Predicate<TType> predicate, bool forceRefresh = false)
        {
            if (_visibleFilter == predicate && !forceRefresh) return;
            _visibleFilter = predicate;

            pnlDisplay.SuspendLayout();
            ClearCache(_contentList);
            if (_contentList.Count > 0)
            {
                pnlDisplay.Height = _contentList.Count(x => x.Visible) * _contentList[0].Control.Height;
            }
            LoadScreenContent();
            pnlDisplay.ResumeLayout();
        }

        public void Sort(IComparer<TType> comparison)
        {
            if (Equals(_comparison, comparison)) return;
            _comparison = comparison;

            pnlDisplay.SuspendLayout();
            ClearCache(_contentList);
            LoadScreenContent();
            pnlDisplay.ResumeLayout();
        }

        private void ContentsChanged(object sender, ListChangedEventArgs eventArgs)
        {
            int intNewIndex = eventArgs?.NewIndex ?? 0;
            List<ControlWithMetaData> lstToRedraw = null;
            switch (eventArgs?.ListChangedType)
            {
                case ListChangedType.ItemChanged:
                    break;
                case ListChangedType.Reset:
                    foreach (ControlWithMetaData objLoopControl in _contentList)
                    {
                        objLoopControl.Cleanup();
                    }
                    _contentList.Clear();
                    foreach (TType objLoopTType in _contents)
                    {
                        _contentList.Add(new ControlWithMetaData(objLoopTType, this));
                    }
                    _indexComparer.Reset(_contents);
                    lstToRedraw = _contentList;
                    break;
                case ListChangedType.ItemAdded:
                    _contentList.Insert(intNewIndex, new ControlWithMetaData(_contents[intNewIndex], this));
                    _indexComparer.Reset(_contents);
                    lstToRedraw = _contentList.GetRange(intNewIndex, _contentList.Count - intNewIndex);
                    break;
                case ListChangedType.ItemDeleted:
                    _contentList[intNewIndex].Cleanup();
                    _contentList.RemoveAt(intNewIndex);
                    _indexComparer.Reset(_contents);
                    lstToRedraw = _contentList;
                    break;
                //case ListChangedType.ItemMoved:
                //    break;

                //case ListChangedType.PropertyDescriptorAdded:
                //    break;
                //case ListChangedType.PropertyDescriptorDeleted:
                //    break;
                //case ListChangedType.PropertyDescriptorChanged:
                //    break;
                default:
                    Utils.BreakIfDebug();
                    break;
            }
            if (lstToRedraw != null && lstToRedraw.Count > 0)
            {
                pnlDisplay.SuspendLayout();
                ChildPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
                ClearCache(lstToRedraw);
                LoadScreenContent();
                pnlDisplay.ResumeLayout();
            }
        }

        private void BindingListDisplay_Scroll(object sender, ScrollEventArgs e)
        {
            pnlDisplay.SuspendLayout();
            LoadScreenContent();
            pnlDisplay.ResumeLayout();
        }

        private void BindingListDisplay_SizeChanged(object sender, EventArgs e)
        {
            pnlDisplay.Width = Width - SystemInformation.VerticalScrollBarWidth;

            if (_contentList == null) //In some edge case i don't know, this is done before _Load()
                pnlDisplay.Height = Height;
            else
                pnlDisplay.Height = _contentList.Count == 0 ? Height : _contentList.Count(x => x.Visible)*_contentList[0].Control.Height;
            foreach (Control control in pnlDisplay.Controls)
            {
                control.Width = pnlDisplay.Width;
            }
        }

        private sealed class ControlWithMetaData
        {
            public TType Item { get; }

            public Control Control => _control ?? (_control = CreateControl());

            private bool ControlCreated => _control != null;

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

            public ControlWithMetaData(TType item, BindingListDisplay<TType> parent)
            {
                _parent = parent;
                Item = item;

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

                if (changes)
                {
                    _parent._resetAtIdle = true;
                }

                _parent.ChildPropertyChanged?.Invoke(sender, e);
            }

            private Control CreateControl()
            {
                Control control = _parent._createFunc(Item);
                control.Visible = false;
                control.Width = _parent.pnlDisplay.Width;
                _parent.pnlDisplay.Controls.Add(control);
                return control;
            }

            public void Reset()
            {
                _visible = null;
                if (ControlCreated)
                {
                    Control.Visible = false;
                    Control.Location = new Point(0, 0);
                    Control.Width = _parent.pnlDisplay.Width;
                }
            }

            public void Cleanup()
            {
                if (ControlCreated)
                {
                    if (Item is INotifyPropertyChanged prop)
                    {
                        prop.PropertyChanged -= item_ChangedEvent;
                    }
                    _parent.pnlDisplay.Controls.Remove(Control);
                    Control.Dispose();
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
                    else
                    {
                        Utils.BreakIfDebug();
                        return 1;
                    }
                }
                else
                {
                    Utils.BreakIfDebug();
                    if (y != null && (x == null || _index.ContainsKey(y)))
                        return -1;

                    return 0;
                }
            }

            public IndexComparer(IList<TType> list)
            {
                Reset(list);
            }

            public void Reset(IList<TType> source)
            {
                _index = new Dictionary<TType, int>();
                for (int i = 0; i < source.Count; i++)
                {
                    _index.Add(source[i], i);
                }
            }
        }
    }
}
