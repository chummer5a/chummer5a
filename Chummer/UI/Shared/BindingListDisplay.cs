using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Chummer.Backend;

namespace Chummer.UI.Shared
{
    public partial class BindingListDisplay<TType> : UserControl
    {
        public PropertyChangedEventHandler ChildPropertyChanged;

        public IComparer<TType> DefaultComparer => _indexComparer;

        private readonly BindingList<TType> _contents; //List of all items supposed to be displayed
        private readonly Func<TType, Control> _createFunc;  //Function to create a control out of a item
        private readonly bool _loadVisibleOnly;
        private List<ControlWithMetaData> _contentList;
        private readonly List<int> _displayIndex = new List<int>();
        private IndexComparer _indexComparer;
        private BitArray _rendered;
        private int _offScreenChunkSize = 1;
        private bool _allRendered;
        private bool _resetAtIdle;
        private Predicate<TType> _visibleFilter = x => true;
        private IComparer<TType> _comparison;

        public BindingListDisplay(BindingList<TType> contents, Func<TType, Control> createFunc, bool loadFast = false, bool loadVisibleOnly = true)
        {
            InitializeComponent();
            _contents = contents;
            _createFunc = createFunc;
            _loadVisibleOnly = loadVisibleOnly;

            if (loadFast)
            {
                InitialSetup();
            }
        }

        private void SkillsDisplay_Load(object sender, EventArgs e)
        {
            DoubleBuffered = true;

            if (_contentList == null)
            {
                InitialSetup();
            }

            Application.Idle += ApplicationOnIdle;
        }

        private void LoadRange(int min, int max, bool blnSuspend = true)
        {
            min = Math.Max(0, min);
            max = Math.Min(_displayIndex.Count, max);
            if (_rendered.FirstMatching(false, min) > max) return;

            for (int i = min; i < max; i++)
            {
                if (_rendered[i]) continue;

                ControlWithMetaData item = _contentList[_displayIndex[i]];

                item.Control.Location = new Point(0, i * _contentList[0].Control.Height);
                item.Control.Visible = true;
                _rendered[i] = true;
            }
        }

        private bool UnrenderedInRange(int min, int max)
        {
            bool any = false;

            for (int i = min; i < max; i++)
            {
                if (_rendered[i] == false) any = true;
            }

            return !any;
        }

        private void InitialSetup()
        {
            pnlDisplay.SuspendLayout();
            SetupContentList();
            ComptuteDisplayIndex();
            LoadScreenContent();
            BindingListDisplay_SizeChanged(null, null);
            pnlDisplay.ResumeLayout();
        }

        private void SetupContentList()
        {
            _contentList = new List<ControlWithMetaData>();
            foreach (TType objLoopTType in _contents)
            {
                _contentList.Add(new ControlWithMetaData(objLoopTType, this));
            }
            _indexComparer = new IndexComparer(_contents);
            if (_comparison == null) _comparison = _indexComparer;
            _contents.ListChanged += ContentsChanged;
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

            _rendered = new BitArray(_displayIndex.Count);
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
            return _contentList.Count == 0 ? 0 :  Math.Min(Height / _contentList[0].Control.Height + 2, _contentList.Count);
        }

        private void ClearAllCache()
        {
            _allRendered = false;
            foreach (ControlWithMetaData item in _contentList)
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
                ClearAllCache();
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
                Log.Info("Offscreen chunck render size increased to " + _offScreenChunkSize);
            }
        }

        public void Filter(Predicate<TType> predicate, bool forceRefresh = false)
        {
            if (_visibleFilter == predicate && !forceRefresh) return;
            _visibleFilter = predicate;

            pnlDisplay.SuspendLayout();
            ClearAllCache();
            if (_contentList.Count > 0)
            {
                pnlDisplay.Height = _contentList.Count(x => x.Visible) * _contentList[0].Control.Height;
            }
            LoadScreenContent();
            pnlDisplay.ResumeLayout();
        }

        public void Sort(IComparer<TType> comparison)
        {
            if (_comparison == comparison) return;
            _comparison = comparison;

            pnlDisplay.SuspendLayout();
            ClearAllCache();
            LoadScreenContent();
            pnlDisplay.ResumeLayout();
        }

        private void ContentsChanged(object sender, ListChangedEventArgs eventArgs)
        {
            _indexComparer.Reset(_contents);
            switch (eventArgs.ListChangedType)
            {
                case ListChangedType.ItemChanged:
                    return;
                //case ListChangedType.Reset:
                //    break;
                case ListChangedType.ItemAdded:
                    _contentList.Insert(eventArgs.NewIndex, new ControlWithMetaData(_contents[eventArgs.NewIndex], this));
                    _indexComparer.Reset(_contents);
                    break;
                case ListChangedType.ItemDeleted:
                    _contentList[eventArgs.NewIndex].Cleanup();
                    _contentList.RemoveAt(eventArgs.NewIndex);
                    _indexComparer.Reset(_contents);
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
            pnlDisplay.ResumeLayout();
            ChildPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
            ClearAllCache();
            LoadScreenContent();
            pnlDisplay.SuspendLayout();
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

            if (_contentList == null) return; //In some edge case i don't know, this is done before _Load()

            pnlDisplay.Height = _contentList.Count == 0 ? Height : _contentList.Count(x => x.Visible)*_contentList[0].Control.Height;
            foreach (Control control in pnlDisplay.Controls)
            {
                control.Width = pnlDisplay.Width - 2;
            }
        }

        private class ControlWithMetaData
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

                INotifyPropertyChanged prop = item as INotifyPropertyChanged;
                if (prop != null)
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
                    _parent.pnlDisplay.Controls.Remove(Control);
            }
        }

        private class IndexComparer : IComparer<TType>
        {
            private Dictionary<TType, int> _index;

            public int Compare(TType x, TType y)
            {
                int xindex;
                if (_index.TryGetValue(x, out xindex))
                {
                    int yindex;
                    if (_index.TryGetValue(y, out yindex))
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
                    if (_index.ContainsKey(y)) return -1;

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
