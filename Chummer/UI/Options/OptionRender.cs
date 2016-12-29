using System.Collections.Generic;
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

        }

        public void SetContents(List<OptionRenderItem> contents)
        {
            if (contents.Count == 0)
            {
                Controls.Clear();
                return;
            }
            bool oldVis = Visible;
            Visible = false;
            Controls.Clear();
            //TODO: Better support for any RenderItems that isnt EntryProxy

            List<OptionItem> entries = contents.OfType<OptionItem>().ToList();
            ControlGroup layouted = PerformGroupLayout(entries, _defaultGroupLayoutProvider);

            for (int i = 0; i < layouted.Controlls.Count; i++)
            {
                System.Drawing.Point p = layouted.Controlls[i].Point;
                p.Offset(5,5);
                Controls.Add(layouted.Controlls[i].Control);
                layouted.Controlls[i].Control.Location = p;
            }


            Visible = oldVis;
        }

        private ControlGroup PerformGroupLayout(List<OptionItem> items, IGroupLayoutProvider layoutProvider)
        {
            ControlGroup ret = new ControlGroup();
            List<Control> controls = items.Select(item => Factories.First(fac => fac.IsSupported(item)).Construct(item)).ToList();

            List<LayoutLineInfo> info = items
                .Select((entry, index) => new LayoutLineInfo
                {
                    ControlSize = controls[index].Size,
                    ControlOffset = controls[index].Location,
                    LayoutString = entry.DisplayString
                })
                .ToList();

            LayoutRenderInfo renderInfo = layoutProvider.PerformLayout(info);

            ret.Controlls = renderInfo.ControlLocations
                .Select((e, i) => new ControlGroup.ControlPoint(){Control = controls[i], Point = e})
                .ToList();

            foreach (TextRenderInfo textInfo in renderInfo.TextLocations)
            {
                //TODO: Remove BackColor assignment
                //Set for debugging purposes to make it easy to see where the labels are.
                Label label = new Label
                {
                    Text = textInfo.Text,
                    Width = textInfo.Size.Width,
                    Height = textInfo.Size.Height,
                    //BackColor = ColorUtilities.RandomColor(),
                    //Padding = new Padding(0,3,0,3)
                };
                if (textInfo.Style != FontStyle.Regular)
                {
                    label.Font = GetCachedFont(textInfo.Style);
                }
                ret.Controlls.Add(new ControlGroup.ControlPoint(){Control = label, Point = textInfo.Location});
            }

            System.Drawing.Point upperLeft = new System.Drawing.Point(
                ret.Controlls.Min(x => x.Point.X),
                ret.Controlls.Min(x => x.Point.Y)
            );

            ret.Bounds = new Rectangle(
                upperLeft.X,
                upperLeft.Y,
                ret.Controlls.Max(x => x.Point.X + x.Control.Width) - upperLeft.X,
                ret.Controlls.Max(x => x.Point.Y + x.Control.Height) - upperLeft.Y
            );

            return ret;
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