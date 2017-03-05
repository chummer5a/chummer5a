using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Chummer.Backend.Options;
using System.Drawing;
using System.Linq;
using Chummer.Backend.Datastructures;
using Chummer.Backend.UI;
using Chummer.UI.Options.ControlGenerators;
using FontStyle = System.Drawing.FontStyle;

namespace Chummer.UI.Options
{
    public class OptionRender : UserControl
    {
        private readonly IGroupLayoutProvider _defaultGroupLayoutProvider;
        private readonly List<PreRenderGroup> _preRenderData = new List<PreRenderGroup>();
        private readonly List<LayoutRenderInfo> _renderData = new List<LayoutRenderInfo>();
        private readonly List<int> _sharedLayoutSpacing = new List<int>();
        // ReSharper disable once PossibleLossOfFraction
        private readonly Font _headerFont = new Font(DefaultFont.FontFamily, FIXED_SPACING * 2 / 3, FontStyle.Bold, GraphicsUnit.Pixel);
        private readonly HowerHelper _howerHelper = new HowerHelper();
        private readonly ToolTip _toolTip = new ToolTip();
        private RTree<string> _toolTipTree = new RTree<string>();
        
        public List<IOptionWinFromControlFactory> Factories { get; set; }

        public OptionRender() : this(new TabAlignmentGroupLayoutProvider())
        {
        }

        public OptionRender(IGroupLayoutProvider layoutProvider)
        {
            _defaultGroupLayoutProvider = layoutProvider;
            IntitializeComponent();
        }

        private void IntitializeComponent()
        {
            AutoScroll = true;
            Size = new Size(300, 200);
            Resize += OnResize;
            _defaultGroupLayoutProvider.LayoutOptions.Font = Font;
            MouseMove += _howerHelper.MouseEventHandler;
            _howerHelper.Hover += OnHover;
            _howerHelper.StopHover += StopHower;

        }

        private void StopHower(object sender, EventArgs e)
        {
            _toolTip.Hide(this);
        }

        private void OnHover(object sender, MouseEventArgs eventArgs)
        {
            string tt = _toolTipTree.Find(eventArgs.Location);
            if(tt != null)
            {_toolTip.Show(tt, this, eventArgs.Location);}
        }

        private void OnResize(object sender, EventArgs eventArgs)
        {
            LayoutGroups();

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);


            //Console.WriteLine("{0}->{1}, {2}", e.ClipRectangle.Top, e.ClipRectangle.Bottom, this.AutoScrollPosition.Y);

            //Can probably also restrict drawing arear...

            for (var index = 0; index < _renderData.Count; index++)
            {
                LayoutRenderInfo renderGroup = _renderData[index];
                e.Graphics.DrawString(
                    _preRenderData[index].Header,
                    _headerFont,
                    new SolidBrush(ForeColor),
                    new PointF(
                        renderGroup.Offset.X + AutoScrollPosition.X,
                        renderGroup.Offset.Y + AutoScrollPosition.Y
                    ));
                foreach (TextRenderInfo renderInfo in renderGroup.TextLocations)
                {
                    e.Graphics.DrawString(
                        renderInfo.Text,
                        GetCachedFont(renderInfo.Style),
                        new SolidBrush(ForeColor),
                        new PointF(
                            renderInfo.Location.X + renderGroup.Offset.X + AutoScrollPosition.X,
                            renderInfo.Location.Y + renderGroup.Offset.Y + AutoScrollPosition.Y + FIXED_SPACING),
                        StringFormat.GenericTypographic);
                }
            }
        }

        public void SetContents(List<OptionRenderItem> contents)
        {
            Stopwatch sw = Stopwatch.StartNew();
            if (contents.Count == 0)
            {
                if (Controls.Count > 0) CleanOldContents();
                return;
            }

            bool oldVis = Visible;
            Visible = false;
            //TODO: Better support for any RenderItems that isnt EntryProxy

            sw.TaskEnd("Initial");

            List<OptionItem> displayEntries = new List<OptionItem>();
            List<OptionRenderItem> nonDisplayEntries = new List<OptionRenderItem>();

            foreach (OptionRenderItem item in contents)
            {
                OptionItem r = item as OptionItem;
                if (r != null)
                    displayEntries.Add(r);
                else
                {
                    if (displayEntries.Count != 0)
                    {
                        _preRenderData.Add(CreatePreRenderGroup(nonDisplayEntries, displayEntries));

                        displayEntries.Clear();
                        nonDisplayEntries.Clear();
                    }

                    nonDisplayEntries.Add(item);
                }
            }

            _preRenderData.Add(CreatePreRenderGroup(nonDisplayEntries, displayEntries));

            sw.TaskEnd("PreRender");

            SetupLayout();

            sw.TaskEnd("Layout");

            Visible = oldVis;
            Invalidate();

            sw.TaskEnd("Last");
        }

        private PreRenderGroup CreatePreRenderGroup(List<OptionRenderItem> nonDisplayEntries, List<OptionItem> displayEntries)
        {

            List<OptionItem> entries = displayEntries
                .Where(o => Factories.Any(x => x.IsSupported(o)))
                .ToList();


            List<Control> controls = entries
                .Select(
                    o => Factories
                        .First(f => f.IsSupported(o))
                        .Construct(o)
                ).ToList();

            foreach (Control control in controls)
            {
                Controls.Add(control);
            }
            PreRenderGroup @group =  new PreRenderGroup
            {
                Controls = controls,
                Lines = entries.Select((x, i) =>
                    {
                        var l = new LayoutLineInfo
                        {
                            ControlOffset = controls[i].Location,
                            ControlSize = controls[i].Size,
                            LayoutString = x.DisplayString,
                        };

                        OptionEntryProxy proxy = x as OptionEntryProxy;
                        if (proxy != null && proxy.ToolTip != null)
                        {
                            l.ToolTip = proxy.ToolTip;
                        }

                        return l;
                    })
                    .ToList()
            };


            foreach (OptionRenderItem displayDirective in nonDisplayEntries)
            {
                HeaderRenderDirective d;
                if ((d = displayDirective as HeaderRenderDirective) != null)
                {
                    group.Header = d.Title;
                }
            }

            return group;
        }

        private void CleanOldContents()
        {
            Controls.Clear();

            _preRenderData.Clear();
            _renderData.Clear();
            _sharedLayoutSpacing.Clear();
        }


        void SetupLayout()
        {
            Graphics g = CreateGraphics();
            _sharedLayoutSpacing.Clear();
            _renderData.Clear();

            foreach (PreRenderGroup preRenderGroup in _preRenderData)
            {
                object cache = _defaultGroupLayoutProvider.ComputeLayoutSpacing(
                    g                    ,
                    preRenderGroup.Lines,
                    _sharedLayoutSpacing
                );
                preRenderGroup.Cache = cache;
            }

            foreach (PreRenderGroup preRenderGroup in _preRenderData)
            {
                LayoutRenderInfo renderGroup = _defaultGroupLayoutProvider.PerformLayout(g,
                    preRenderGroup.Lines, _sharedLayoutSpacing, preRenderGroup.Cache);


                _renderData.Add(renderGroup);
            }

            LayoutGroups();
        }

        private void LayoutGroups()
        {
            _toolTipTree = new RTree<string>(); //No clear method

            PointF offset = PointF.Empty; //new PointF(5, 5);
            int widestColumn = _renderData.Count > 0 ? _renderData.Max(x => x.Width) : 1;
            int columnCount = (Width - 5) / (widestColumn + 5);
            if (columnCount == 0) columnCount = 1;

            int[] usedSpace = new int[columnCount];
            for (int i = 0; i < usedSpace.Length; i++)
                usedSpace[i] = 5;

            foreach (LayoutRenderInfo renderInfo in _renderData)
            {
                int smallest = 0;
                for (int i = 0; i < usedSpace.Length; i++)
                {
                    if (usedSpace[i] < usedSpace[smallest])
                        smallest = i;
                }

                renderInfo.Offset = new Point(smallest * (widestColumn + 5) + 5, usedSpace[smallest]);
                usedSpace[smallest] += renderInfo.Height + FIXED_SPACING;
            }

            for (var index = 0; index < _preRenderData.Count; index++)
            {
                PreRenderGroup preRenderGroup = _preRenderData[index];
                LayoutRenderInfo renderGroup = _renderData[index];
                for (int i = 0; i < preRenderGroup.Controls.Count; i++)
                {
                    Point controlPoint = new Point(
                        renderGroup.ControlLocations[i].X + (int) offset.X + renderGroup.Offset.X,
                        renderGroup.ControlLocations[i].Y + (int) offset.Y + renderGroup.Offset.Y + FIXED_SPACING);


                    preRenderGroup.Controls[i].Location = controlPoint;
                }

                foreach (ToolTipData toolTip in renderGroup.ToolTips)
                {
                    Rectangle r = toolTip.Location;
                    r.Y += FIXED_SPACING;
                    _toolTipTree.Insert(toolTip.Text, r);
                }
            }


        }

        private readonly Dictionary<FontStyle, Font> _fontCache = new Dictionary<FontStyle, Font>();
        private const int FIXED_SPACING = 30;

        private Font GetCachedFont(FontStyle textInfoStyle)
        {
            Font font;
            if (_fontCache.TryGetValue(textInfoStyle, out font))
            {
                return font;
            }

            font = new Font(Font, textInfoStyle);
            _fontCache[textInfoStyle] = font;
            return font;
        }


        class PreRenderGroup
        {
            public List<Control> Controls { get; set; }
            public List<LayoutLineInfo> Lines { get; set; }
            public object Cache { get; set; }
            public string Header { get; set; }
        }
    }
}