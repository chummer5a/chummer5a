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
            base.OnPaint(e);

            Stopwatch sw = Stopwatch.StartNew();
            //We don't quite know what has changed, so for now i'm just going to redo everything.
            //If a profiler points this out as a performance problem someday, lots of stuff can probably be cached.
            //Can probably also restrict drawing arear...

            PointF offset = new PointF(5, 5);

            if (renderData == null)
            {
                sharedLayoutSpacing = new List<int>();
                renderData = new List<LayoutRenderInfo>();

                foreach (PreRenderGroup preRenderGroup in preRenderData)
                {
                    _defaultGroupLayoutProvider.ComputeLayoutSpacing(
                        e.Graphics,
                        preRenderGroup.Lines,
                        sharedLayoutSpacing
                    );
                }

                foreach (PreRenderGroup preRenderGroup in preRenderData)
                {
                    LayoutRenderInfo renderGroup = _defaultGroupLayoutProvider.PerformLayout(e.Graphics,
                        preRenderGroup.Lines, sharedLayoutSpacing);

                    for (int i = 0; i < preRenderGroup.Controls.Count; i++)
                    {
                        preRenderGroup.Controls[i].Location =
                            new System.Drawing.Point(renderGroup.ControlLocations[i].X + (int) offset.X,
                                renderGroup.ControlLocations[i].Y + (int) offset.Y);
                    }

                    renderData.Add(renderGroup);
                }
                //In theory: Get sizes of each preRenderGroup (job for _defaultGroupLayoutProvider)
                //and make a "big" layout class do a layout of each group

                //This would be the location of each layoutgroup, but such thing don't exist...

                sw.TaskEnd("Render setup");
            }


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
            if (contents.Count == 0)
            {
                if (Controls.Count > 0) CleanOldContents();
                return;
            }

            bool oldVis = Visible;
            Visible = false;
            //TODO: Better support for any RenderItems that isnt EntryProxy

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

            foreach (Control control in controls)
            {
                Controls.Add(control);
            }

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

            Visible = oldVis;
            this.Invalidate();
        }

        private void CleanOldContents()
        {
            Controls.Clear();

            preRenderData.Clear();
            preRenderData = null;
            renderData.Clear();
            sharedLayoutSpacing.Clear();
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