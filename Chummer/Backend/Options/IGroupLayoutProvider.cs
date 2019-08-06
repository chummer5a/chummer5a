using System.Collections.Generic;
using System.Drawing;

namespace Chummer.Backend.Options
{
    public interface IGroupLayoutProvider<LayoutEntry> where LayoutEntry : ILayoutEntry
    {
        /// <summary>
        /// Preliminary computation for layouts. Split from final layouts to allow multiple layouts to match
        /// </summary>
        /// <param name="willBeRenderedOn">A Graphics object for the final layout to be rendered on, that allows the size of text to be computed</param>
        /// <param name="contents">All lines that shall be rendered</param>
        /// <param name="crossGroupSharedData">An object that helps ensure consistent look between multiple groups</param>
        /// <returns></returns>
        LayoutGroupComputation ComputeLayoutGroup(
            Graphics willBeRenderedOn, 
            List<LayoutEntry> contents,
            ref object crossGroupSharedData);


        RenderedLayoutGroup RenderLayoutGroup(
            Graphics willBeRenderedOn, 
            LayoutGroupComputation preComputedLayoutGroup, 
            object crossGroupSharedData = null);


        //object ComputeLayoutSpacing(Graphics rendergarget, List<LayoutLineInfo> contents, List<int> additonalConformTarget = null);
        //RenderedLayoutGroup PerformLayout(Graphics renderGraphics, List<LayoutLineInfo> contents, List<int> preComputedLayoutSpacing, object CachedCompute);
        LayoutOptionsContainer LayoutOptions { get; set; }

        
    }

    public abstract class LayoutGroupComputation { }

    public class ControlLayoutEntry : ILayoutEntry
    {
        public System.Windows.Forms.Control Control { get; set; }
        public string LayoutString { get; set; }
        public string ToolTip { get; set; }

        public string OriginName { get; set; }

        public Rectangle GetRectangle() => new Rectangle(Control.Location, Control.Size);
    }

    public interface ILayoutEntry
    {
        /// <summary>
        /// The rectangle specifying the size of the embedded gui element
        /// </summary>
        /// <returns></returns>
        Rectangle GetRectangle();

        /// <summary>
        /// A layout string containing a {} to be replaced with a gui element
        /// </summary>
        string LayoutString { get; }

        /// <summary>
        /// A string describing the origin of this ILayoutEntry
        /// </summary>
        string OriginName { get; }

        /// <summary>
        /// An optional tooltip for the entry
        /// </summary>
        string ToolTip { get;}

    }

    public class RenderedLayoutGroup
    {
        public List<GraphicRenderInfo> ControlLocations { get; set; }
        public List<TextRenderInfo> TextLocations { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Point Offset { get; set; }
        public List<ToolTipData> ToolTips { get; set; }

        public class ToolTipData
        {
            public string Text { get; }
            public Rectangle Location { get; }

            public ToolTipData(string text, Rectangle location)
            {
                Text = text;
                Location = location;
            }
        }

        public class TextRenderInfo
        {
            public Point Location { get; set; }
            public Size Size { get; set; }
            public string Text { get; set; }
            public FontStyle Style { get; set; }
        }

        public class GraphicRenderInfo
        {
            public ILayoutEntry Source { get; set; }
            public Point Location { get; set; }
        }
    }

    public class LayoutOptionsContainer
    {
        public int Linespacing { get; set; } = 6;
        public int ControlMargin { get; set; } = 3;
        public Font Font { get; set; }
    }
}
