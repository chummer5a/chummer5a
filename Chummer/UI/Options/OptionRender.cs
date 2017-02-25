using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using Chummer.Backend.Options;
using System.Drawing;
using System.Linq;
using Chummer.Backend;
using Chummer.UI.Options.ControlGenerators;
using FontStyle = System.Drawing.FontStyle;

namespace Chummer.UI.Options
{
    public class OptionRender : UserControl
    {
        private readonly IGroupLayoutProvider _defaultGroupLayoutProvider;
        private List<PreRenderGroup> preRenderData = null;
        private List<LayoutRenderInfo> renderData = null;
        private List<int> sharedLayoutSpacing;
        
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
            Size = new System.Drawing.Size(484, 354);

            _defaultGroupLayoutProvider.LayoutOptions.Font = Font;

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            PointF offset = new PointF(5, 5);
            Stopwatch sw = Stopwatch.StartNew();
            base.OnPaint(e);

            sw.TaskEnd("base");

            //We don't quite know what has changed, so for now i'm just going to redo everything.
            //If a profiler points this out as a performance problem someday, lots of stuff can probably be cached.
            //Can probably also restrict drawing arear...

            foreach (LayoutRenderInfo renderGroup in renderData)
            {
                foreach (TextRenderInfo renderInfo in renderGroup.TextLocations)
                {
                    e.Graphics.DrawString(renderInfo.Text, GetCachedFont(renderInfo.Style), new SolidBrush(ForeColor), new PointF(renderInfo.Location.X + offset.X, renderInfo.Location.Y + offset.Y), StringFormat.GenericTypographic);
                }
            }

            sw.TaskEnd("Render");

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

            List<OptionItem> entries = contents
                .OfType<OptionItem>()
                .Where(o => Factories.Any(x => x.IsSupported(o)))
                .ToList();


            List<Control> controls = entries
                .Select(
                    o => Factories
                        .First(f => f.IsSupported(o))
                        .Construct(o)
                ).ToList();

            sw.TaskEnd("CreateControl");

            foreach (Control control in controls)
            {
                Controls.Add(control);
            }

            sw.TaskEnd("AddControl");

            preRenderData = new List<PreRenderGroup>()
            {
                new PreRenderGroup
                {
                    Controls = controls,
                    Lines = entries.Select((x, i) => new LayoutLineInfo
                    {
                        ControlOffset = controls[i].Location,
                        ControlSize = controls[i].Size,
                        LayoutString = x.DisplayString
                    }).ToList()
                }
            };

            sw.TaskEnd("PreRender");

            SetupLayout();

            sw.TaskEnd("Layout");

            Visible = oldVis;
            this.Invalidate();

            sw.TaskEnd("Last");
        }

        private void CleanOldContents()
        {
            Controls.Clear();

            preRenderData.Clear();
            preRenderData = null;
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

                Console.WriteLine("Controls = {0}, locations = {1}", preRenderGroup.Controls.Count, renderGroup.ControlLocations.Count);
                for (int i = 0; i < preRenderGroup.Controls.Count; i++)
                {
                    preRenderGroup.Controls[i].Location =
                        new System.Drawing.Point(renderGroup.ControlLocations[i].X + (int) offset.X,
                            renderGroup.ControlLocations[i].Y + (int) offset.Y);
                }

                renderData.Add(renderGroup);
            }

            Console.WriteLine("Exit");
            //In theory: Get sizes of each preRenderGroup (job for _defaultGroupLayoutProvider)
            //and make a "big" layout class do a layout of each group

            //This would be the location of each layoutgroup, but such thing don't exist...
        }

        private readonly Dictionary<FontStyle, Font> _fontCache = new Dictionary<FontStyle, Font>();
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
        }
    }

    internal class ControlGroup
    {
        public List<ControlPoint> Controlls { get; set; }
        public Rectangle Bounds { get; set; }

        internal class ControlPoint
        {
            public Control Control { get; set; }
            public System.Drawing.Point Point { get; set; }
        }
    }
}