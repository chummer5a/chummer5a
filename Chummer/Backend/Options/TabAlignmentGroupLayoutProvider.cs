using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Point = System.Drawing.Point;

namespace Chummer.Backend.Options
{
    public class TabAlignmentGroupLayoutProvider : IGroupLayoutProvider
    {
        public LayoutOptionsContainer LayoutOptions { get; set; } = new LayoutOptionsContainer();

        public LayoutRenderInfo PerformLayout(List<LayoutLineInfo> contents)
        {
            LayoutRenderInfo ret = new LayoutRenderInfo(){ControlLocations = new List<Point>(), TextLocations = new List<TextRenderInfo>()};
            List<int> alignmentLenghts = new List<int>();

            foreach (LayoutLineInfo line in contents)
            {
                string[] alignmentPieces = line.LayoutString.Split(new []{"\\t"}, StringSplitOptions.None);
                Console.WriteLine($"splitting {line.LayoutString} into [{string.Join(", ", alignmentPieces)}]");
                for (int i = 0; i < alignmentPieces.Length-1; i++)
                {
                    Console.WriteLine("Processing {0}", alignmentPieces[i]);
                    if (alignmentLenghts.Count > i)
                    {

                        alignmentLenghts[i] = Math.Max(
                            alignmentLenghts[i],
                            ElementSize(alignmentPieces[i], line.ControlSize, line.ControlOffset).Width);
                    }
                    else
                    {
                        Console.WriteLine($"Creating new align on \"{alignmentPieces[i]}\"{i}");
                        alignmentLenghts.Add(ElementSize(alignmentPieces[i], line.ControlSize, line.ControlOffset).Width);
                    }
                }
            }

            alignmentLenghts.Insert(0,0);
            Console.WriteLine(string.Join(", ", alignmentLenghts));

            for (int i = 1; i < alignmentLenghts.Count; i++)
            {
                alignmentLenghts[i] += alignmentLenghts[i - 1];
                Console.WriteLine($"[{i}] <- {alignmentLenghts[i]} added {alignmentLenghts[i - 1]}");
            }

            int lineTop = 0;
            int lineBottom = 0;

            foreach (LayoutLineInfo line in contents)
            {
                bool drewControl = false;
                int lineRight = 0;
                string[] alignmentPieces = line.LayoutString.Split(new []{"\\t"}, StringSplitOptions.None);
                for (int i = 0; i < alignmentPieces.Length; i++)
                {
                    if (alignmentPieces[i].Contains("{}"))
                    {
                        string[] sides = alignmentPieces[i].Split(split, 2, StringSplitOptions.None);
                        Size size = ElementSize(sides[0], Size.Empty, Point.Empty);
                        ret.TextLocations.Add(new TextRenderInfo{Location = new Point(alignmentLenghts[i], lineTop), Size = size, Text = sides[0]});

                        Size size2 = ElementSize(sides[1], Size.Empty, Point.Empty);


                        Console.WriteLine($"text left = {alignmentLenghts[i]} + {size.Width} + {line.ControlSize.Width} + {LayoutOptions.ControlMargin} * 2");
                        ret.TextLocations.Add(new TextRenderInfo
                        {
                            Location =
                                new Point(
                                    alignmentLenghts[i] + size.Width + line.ControlSize.Width +
                                    LayoutOptions.ControlMargin * 2, lineTop),
                            Size = size2,
                            Text = sides[1]
                        });

                        ret.ControlLocations.Add(new Point(alignmentLenghts[i] + size.Width + LayoutOptions.ControlMargin, lineTop + line.ControlOffset.Y));
                        Console.WriteLine($"c = {alignmentLenghts[i]} + {size.Width} + {LayoutOptions.ControlMargin}");

                        lineBottom = Math.Max(lineBottom, lineTop + Math.Max(size.Height, line.ControlSize.Height - Math.Min(0, line.ControlOffset.Y)));
                        drewControl = true;
                    }
                    else
                    {
                        Size size = ElementSize(alignmentPieces[i], Size.Empty, Point.Empty);
                        TextRenderInfo tri = new TextRenderInfo
                        {
                            Location = new Point(alignmentLenghts[i], lineTop),
                            Size = size,
                            Text = alignmentPieces[i]
                        };
                        Console.WriteLine($"Rendering tri \"{tri.Text}\" at {tri.Location.X},{tri.Location.Y} ({tri.Size.Width},{tri.Size.Height})");
                        ret.TextLocations.Add(tri);
                        lineBottom = Math.Max(lineBottom, lineTop + size.Height);

                        lineRight = alignmentLenghts[i] + size.Width;
                    }
                }

                if(drewControl == false)
                {
                    ret.ControlLocations.Add(new Point(lineRight + LayoutOptions.ControlMargin, lineTop + line.ControlOffset.Y));
                    lineBottom = Math.Max(lineBottom, lineTop + line.ControlSize.Height - Math.Min(0, line.ControlOffset.Y));
                }

                lineTop = lineBottom + LayoutOptions.Linespacing;
            }





            return ret;
        }


        private string[] split = new[] {"{}"};
        private Size ElementSize(string textMaybeEmbeddedControl, Size controlSize, Point controlOffset)
        {
            if (textMaybeEmbeddedControl.Contains("{}"))
            {
                string[] sides = textMaybeEmbeddedControl.Split(split, 2, StringSplitOptions.None);

                Size s1, s2;
                s1 = TextRenderer.MeasureText(sides[0], Control.DefaultFont);
                s2 = TextRenderer.MeasureText(sides[1], Control.DefaultFont);

                //TODO: this should be how far the total element goes outside the upper left cornor of the text. Any that the control goes over should be ignored.
                //Probably confused because i don't quite see what controlOffset.X means
                int height = Math.Max(s1.Height, controlSize.Height - Math.Min(0, controlOffset.Y));
                int width = controlSize.Width + s1.Width + s2.Width; //Probably sane way
                Console.WriteLine($"Calculated width = {width} from {controlSize.Width} +{s1.Width} + {s2.Width} \"{textMaybeEmbeddedControl}\"");
                return new Size(width /*-5*/, height);
            }
            else
            {
                Size s = TextRenderer.MeasureText(textMaybeEmbeddedControl, Control.DefaultFont);
                //s.Width -= 5;
                return s;
            }
        }

        public class LayoutOptionsContainer
        {
            public int Linespacing { get; set; } = 6;
            public int ControlMargin { get; set; } = 3;
        }
    }
}