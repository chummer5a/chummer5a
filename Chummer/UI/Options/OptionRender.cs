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
        private readonly IGroupLayoutProvider<ControlLayoutEntry> _defaultGroupLayoutProvider;
        private readonly List<PreRenderGroup> _preRenderData = new List<PreRenderGroup>();
        private readonly List<RenderedLayoutGroup> _renderData = new List<RenderedLayoutGroup>();
        // ReSharper disable once PossibleLossOfFraction
        private readonly Font _headerFont = new Font(DefaultFont.FontFamily, FIXED_SPACING * 2 / 3, FontStyle.Bold, GraphicsUnit.Pixel);
        private readonly HoverHelper _hoverHelper = new HoverHelper();
        private readonly ToolTip _toolTip = new ToolTip();
        private RTree<string> _toolTipTree = new RTree<string>();
        private readonly Graphics _objGraphics;

        public List<IOptionWinFromControlFactory> Factories { get; set; }

        public OptionRender() : this(new TabAlignmentGroupLayoutProvider<ControlLayoutEntry>())
        {
            _objGraphics = CreateGraphics();
        }

        public OptionRender(IGroupLayoutProvider<ControlLayoutEntry> layoutProvider)
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
            MouseMove += _hoverHelper.MouseEventHandler;
            _hoverHelper.Hover += OnHover;
            _hoverHelper.StopHover += StopHover;

        }

        private void StopHover(object sender, EventArgs e)
        {
            _toolTip.Hide(this);
        }

        private void OnHover(object sender, MouseEventArgs eventArgs)
        {
            string tt = _toolTipTree.Find(eventArgs.Location);
            if (tt != null)
            {
                _toolTip.Show(tt, this, eventArgs.Location);
            }
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
            SolidBrush objBrush = new SolidBrush(ForeColor);
            for (var index = 0; index < _renderData.Count; index++)
            {
                RenderedLayoutGroup renderGroup = _renderData[index];
                int fltRenderGroupX = renderGroup.Offset.X + AutoScrollPosition.X;
                int fltRenderGroupY = renderGroup.Offset.Y + AutoScrollPosition.Y;

                TextRenderer.DrawText(
                    e.Graphics, 
                    _preRenderData[index].Header, 
                    _headerFont, 
                    new Point(
                        fltRenderGroupX, 
                        fltRenderGroupY), 
                    ForeColor );

                foreach (RenderedLayoutGroup.TextRenderInfo renderInfo in renderGroup.TextLocations)
                {
                    ////Draw a rectangle under text to show what render is doing
                    //e.Graphics.DrawRectangle(
                    //    new Pen(Color.Aqua), 
                    //    renderInfo.Location.X + fltRenderGroupX,
                    //    renderInfo.Location.Y + fltRenderGroupY + FIXED_SPACING, 
                    //    renderInfo.Size.Width, 
                    //    renderInfo.Size.Height);

                    TextRenderer.DrawText(
                        e.Graphics, 
                        renderInfo.Text, 
                        GetCachedFont(renderInfo.Style), 
                        new Point(
                            renderInfo.Location.X + fltRenderGroupX, 
                            renderInfo.Location.Y + fltRenderGroupY + FIXED_SPACING), 
                        ForeColor);
                }
            }
        }

        public void SetContents(List<OptionRenderItem> contents)
        {
            Stopwatch sw = Stopwatch.StartNew();
            CleanOldContents();
            if (contents.Count == 0)
            {
                return;
            }

            bool oldVis = Visible;
            Visible = false;
            //TODO: Better support for any RenderItems that isnt EntryProxy

            sw.TaskEnd("Initial");

            List<OptionItem> displayEntries = new List<OptionItem>();
            HeaderRenderDirective objLoopRenderDirective = null;

            //This is simply grouping items, where headers and following items are grouped.
            //To employ a methaphor, it creates a chapter data structure, based only one the font size of each line, this way grouping title and its following lines
            foreach (OptionRenderItem item in contents)
            {
                OptionItem r = item as OptionItem;
                if (r != null)
                    displayEntries.Add(r);
                else
                {
                    if (displayEntries.Count != 0)
                    {
                        _preRenderData.Add(CreatePreRenderGroup(objLoopRenderDirective, displayEntries));

                        displayEntries.Clear();
                    }

                    objLoopRenderDirective = item as HeaderRenderDirective;
                }
            }

            _preRenderData.Add(CreatePreRenderGroup(objLoopRenderDirective, displayEntries));

            sw.TaskEnd("PreRender");

            SetupLayout();

            sw.TaskEnd("Layout");

            Visible = oldVis;
            Invalidate();

            sw.TaskEnd("Last");
        }

        private PreRenderGroup CreatePreRenderGroup(HeaderRenderDirective objRenderDirective, List<OptionItem> displayEntries)
        {
            var lstControlsToRender = new List<Control>(displayEntries.Count);
            var lines = new List<ControlLayoutEntry>();

            for (int i = 0; i < displayEntries.Count; i++)
            {
                OptionItem entry = displayEntries[i];
                IOptionWinFromControlFactory factory = Factories.FirstOrDefault(x => x.IsSupported(entry));

                if (factory == null) continue;

                Control control = factory.Construct(entry);

                OptionEntryProxy entryAsProxy = entry as OptionEntryProxy;

                //It felt less insane to recreate the "Key" that this would look up here than when the key was generated
                //and doing it the right way and actually including it in the OptionEntryProxy didn't feel good either, but this is a strange, rather undocumented dependency
                //shame on me
                string originName = entryAsProxy?.TargetProperty?.Name;
                if (originName != null) originName = "Display_" + originName;
                
                ControlLayoutEntry line = new ControlLayoutEntry
                {
                    Control = control,
                    LayoutString = entry.DisplayString,
                    ToolTip = entryAsProxy?.ToolTip,
                    OriginName = originName
                };

                //NB Big and Small C. One is controls in this control, other is controls that the render can play with
                Controls.Add(control);
                lines.Add(line);
            }

            PreRenderGroup @group =  new PreRenderGroup
            {
                Header = objRenderDirective?.Title ?? string.Empty,
                Lines = lines
            };

            return group;
        }

        private void CleanOldContents()
        {
            Controls.Clear();

            _preRenderData.Clear();
        }

        void SetupLayout()
        {
            object share = null;
            _renderData.Clear();

            List<LayoutGroupComputation> layouts = new List<LayoutGroupComputation>();

            foreach (PreRenderGroup preRenderGroup in _preRenderData)
            {
                layouts.Add(_defaultGroupLayoutProvider.ComputeLayoutGroup(
                    _objGraphics,
                    preRenderGroup.Lines,
                    ref share
                ));
            }

            foreach (LayoutGroupComputation layoutGroupComputation in layouts)
            {
                _renderData.Add(_defaultGroupLayoutProvider.RenderLayoutGroup(_objGraphics, layoutGroupComputation, share));
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
            for (int i = 0; i < columnCount; i++)
                usedSpace[i] = 5;

            foreach (RenderedLayoutGroup renderInfo in _renderData)
            {
                int smallest = 0;
                for (int i = 0; i < columnCount; i++)
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
                RenderedLayoutGroup renderGroup = _renderData[index];
                int intRenderGroupX = renderGroup.Offset.X + (int)offset.X;
                int intRenderGroupY = renderGroup.Offset.Y + (int)offset.Y;
                //If you get a crash at this point, make sure any new options you've added have a Display_{name} entry in en-us.xml. Check casing!
                for (int i = 0; i < renderGroup.ControlLocations.Count; i++)
                {
                    var objLoopPoint = renderGroup.ControlLocations[i];
                    Point controlPoint = new Point(
                        objLoopPoint.Location.X + intRenderGroupX,
                        objLoopPoint.Location.Y + intRenderGroupY + FIXED_SPACING);


                    ((ControlLayoutEntry)objLoopPoint.Source).Control.Location = controlPoint;
                }

                foreach (RenderedLayoutGroup.ToolTipData toolTip in renderGroup.ToolTips)
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
            public List<ControlLayoutEntry> Lines { get; set; }
            public object Cache { get; set; }
            public string Header { get; set; }
        }
    }
}
