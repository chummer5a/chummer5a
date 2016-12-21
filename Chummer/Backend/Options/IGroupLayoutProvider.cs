using System.Collections.Generic;
using System.Drawing;

namespace Chummer.Backend.Options
{
    public interface IGroupLayoutProvider
    {
        LayoutRenderInfo PerformLayout(List<LayoutLineInfo> contents);
    }

    public class LayoutLineInfo
    {
        public Size ControlSize { get; set; }
        public Point ControlOffset { get; set; }
        public string LayoutString { get; set; }
    }

    public class LayoutRenderInfo
    {
        public List<Point> ControlLocations { get; set; }
        public List<TextRenderInfo> TextLocations { get; set; }
    }

    public class TextRenderInfo
    {
        public Point Location { get; set; }
        public Size Size { get; set; }
        public string Text { get; set; }
        public FontStyle Style { get; set; }
    }
}