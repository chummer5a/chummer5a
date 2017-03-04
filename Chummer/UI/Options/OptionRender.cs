using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using Chummer.Backend.Options;
using System.Drawing;
using System.Linq;
using System.Resources;
using Chummer.Backend;
using Chummer.UI.Options.ControlGenerators;
using FontStyle = System.Drawing.FontStyle;

namespace Chummer.UI.Options
{
    public class OptionRender : UserControl
    {
        private readonly IGroupLayoutProvider _defaultGroupLayoutProvider;
        private List<PreRenderGroup> preRenderData = new List<PreRenderGroup>();
        private List<LayoutRenderInfo> renderData = null;
        private List<int> sharedLayoutSpacing;
        private Font HeaderFont = new Font(Control.DefaultFont.FontFamily, (FIXED_SPACING * 2) / 3, FontStyle.Bold, GraphicsUnit.Pixel);
        
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
            Size = new System.Drawing.Size(300, 200);
            this.Resize += OnResize;
            _defaultGroupLayoutProvider.LayoutOptions.Font = Font;

        }

        private void OnResize(object sender, EventArgs eventArgs)
        {
            LayoutGroups();

            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);


            //Console.WriteLine("{0}->{1}, {2}", e.ClipRectangle.Top, e.ClipRectangle.Bottom, this.AutoScrollPosition.Y);

            //Can probably also restrict drawing arear...

            for (var index = 0; index < renderData.Count; index++)
            {
                LayoutRenderInfo renderGroup = renderData[index];
                e.Graphics.DrawString(
                    preRenderData[index].Header,
                    HeaderFont,
                    new SolidBrush(ForeColor),
                    new PointF(
                        renderGroup.Offset.X + AutoScrollPosition.X,
                        renderGroup.Offset.Y + AutoScrollPosition.Y
                    ));
                Console.WriteLine("Offset = {0}", renderGroup.Offset);
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
                        preRenderData.Add(CreatePreRenderGroup(nonDisplayEntries, displayEntries));

                        displayEntries.Clear();
                        nonDisplayEntries.Clear();
                        Console.WriteLine("Added new group");
                    }

                    nonDisplayEntries.Add(item);
                }
            }

            preRenderData.Add(CreatePreRenderGroup(nonDisplayEntries, displayEntries));

            sw.TaskEnd("PreRender");

            SetupLayout();

            sw.TaskEnd("Layout");

            Visible = oldVis;
            this.Invalidate();

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
                Lines = entries.Select((x, i) => new LayoutLineInfo
                    {
                        ControlOffset = controls[i].Location,
                        ControlSize = controls[i].Size,
                        LayoutString = x.DisplayString
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

            preRenderData.Clear();
            renderData.Clear();
            sharedLayoutSpacing.Clear();
        }


        void SetupLayout()
        {
            Console.WriteLine("Enter");
            PointF offset = new PointF(5, 5);
            Graphics g = this.CreateGraphics();
            sharedLayoutSpacing = new List<int>();
            renderData = new List<LayoutRenderInfo>();

            foreach (PreRenderGroup preRenderGroup in preRenderData)
            {
                object cache = _defaultGroupLayoutProvider.ComputeLayoutSpacing(
                    g                    ,
                    preRenderGroup.Lines,
                    sharedLayoutSpacing
                );
                preRenderGroup.Cache = cache;
            }

            Console.WriteLine("First");
            foreach (PreRenderGroup preRenderGroup in preRenderData)
            {
                LayoutRenderInfo renderGroup = _defaultGroupLayoutProvider.PerformLayout(g,
                    preRenderGroup.Lines, sharedLayoutSpacing, preRenderGroup.Cache);


                renderData.Add(renderGroup);
            }

            Console.WriteLine("Exit");

            LayoutGroups();
        }

        private void LayoutGroups()
        {
            PointF offset = PointF.Empty; //new PointF(5, 5);
            int widestColumn = renderData.Count > 0 ? renderData.Max(x => x.Width) : 1;
            int columnCount = (Width - 5) / (widestColumn + 5);
            if (columnCount == 0) columnCount = 1;
            Console.WriteLine($"Width of {Width} and a size of {widestColumn} gives space for {columnCount}");

            int[] usedSpace = new int[columnCount];
            for (int i = 0; i < usedSpace.Length; i++)
                usedSpace[i] = 5;

            foreach (LayoutRenderInfo renderInfo in renderData)
            {
                int smallest = 0;
                for (int i = 0; i < usedSpace.Length; i++)
                {
                    if (usedSpace[i] < usedSpace[smallest])
                        smallest = i;
                }

                renderInfo.Offset = new System.Drawing.Point(smallest * (widestColumn + 5) + 5, usedSpace[smallest]);
                usedSpace[smallest] += renderInfo.Height + FIXED_SPACING;
            }

            for (var index = 0; index < preRenderData.Count; index++)
            {
                PreRenderGroup preRenderGroup = preRenderData[index];
                LayoutRenderInfo renderGroup = renderData[index];
                Console.WriteLine("Controls = {0}, locations = {1}", preRenderGroup.Controls.Count,
                    renderGroup.ControlLocations.Count);
                for (int i = 0; i < preRenderGroup.Controls.Count; i++)
                {
                    preRenderGroup.Controls[i].Location =
                        new System.Drawing.Point(
                            renderGroup.ControlLocations[i].X + (int) offset.X + renderGroup.Offset.X,
                            renderGroup.ControlLocations[i].Y + (int) offset.Y + renderGroup.Offset.Y + FIXED_SPACING);
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