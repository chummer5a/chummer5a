using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Chummer.UI.Options;

namespace Chummer.Backend.UI
{
    public class BookImageManager
    {
        public Color BackColor = SystemColors.Control;
        public Color GlowColor = Color.Blue;

        public int GlowBorder
        {
            get { return _glowBorder; }
            set
            {
                if(value < 0) throw new ArgumentOutOfRangeException();
                _glowBorder = value;
            }
        }

        Dictionary<int, Image> _cache = new Dictionary<int, Image>();
        private int _glowBorder = 20;

        public Image GetImage(string bookCode, bool selected, bool aura)
        {
            int hash = Hash(bookCode , selected , aura );
            Image image;
            if (!_cache.TryGetValue(hash, out image))
            {
                image = GenerateImage(bookCode, selected, aura);
                _cache.Add(hash, image);
            }

            return image;


        }

        private Image GenerateImage(string bookCode, bool enabled, bool aura)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Bitmap source = GetBaseImage(bookCode);

            //Comming soon: Bitmap checkbox (un)checked


            BitmapData sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            //This might create some _weird_ errors if somebody tries to run it on another endianess. ARM?
            int[] sourceArray = new int[sourceData.Width* sourceData.Height];

            int realWidth = (source.Width + GlowBorder * 2);
            int[] destinationArray = new int[realWidth * (source.Height + GlowBorder * 2)];

            Marshal.Copy(sourceData.Scan0, sourceArray, 0, Math.Abs(sourceData.Stride) * sourceData.Height / sizeof(int));
            source.UnlockBits(sourceData);

            int backcolor = ColorToInt(BackColor);
            int auracolor = ColorToInt(GlowColor);

            //Copy a color over all image
            for (int i = 0; i < destinationArray.Length; i++)
            {
                destinationArray[i] = backcolor;
            }

            Func<int, int> convert;
            if (enabled)
            {
                convert = Pass;
            }
            else
            {
                convert = ColorIntToGreyscale;
            }

            //Copy main image
            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    int color = convert(sourceArray[y * source.Width + x]);
                    //Color preprocessing here
                    destinationArray[(y + GlowBorder) * realWidth + GlowBorder + x] = color;
                }
            }

            //Copy checkbox

            //create aura
            for (int y = 0; y < GlowBorder; y++)
            {

                int col = aura? ColorUtilities.ARGBIntLABInterpolate(auracolor,backcolor, y / (float) GlowBorder) : backcolor;
                for (int x = 0; x < source.Width; x++)
                {
                    destinationArray[(y * realWidth) + GlowBorder + x] = col;
                }
            }


            Bitmap final = new Bitmap(source.Width + GlowBorder * 2, source.Height + GlowBorder * 2);
            BitmapData destinationData = final.LockBits(new Rectangle(0, 0, final.Width, final.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            Marshal.Copy(destinationArray, 0, destinationData.Scan0, destinationArray.Length);
            final.UnlockBits(destinationData);


            Console.WriteLine($"Image generated in {sw.Elapsed.TotalMilliseconds}ms");
            return final;
        }

        private int IntColorInterpolate(int color1, int color2, float scale)
        {
            int red1 = (color1 & 0x00ff0000) >> 16;
            int green1 = (color1 & 0x0000ff00) >> 8;
            int blue1= (color1 & 0x000000ff) ;
            int alpha1 = (color1 >> 24);

            int red2 = (color2 & 0x00ff0000) >> 16;
            int green2 = (color2 & 0x0000ff00) >> 8;
            int blue2 = (color2 & 0x000000ff) ;
            int alpha2 = (color2  >> 24);

            return (IntInterpolate(alpha1, alpha2, scale) << 24) | (IntInterpolate(red1, red2, scale) << 16) |
                   (IntInterpolate(green1, green2, scale) << 8) | (IntInterpolate(blue1, blue2, scale));


        }

        private int IntInterpolate(int i1, int i2, float scale)
        {
            int q = (int) (scale * 100);
            int p = 100 - q;

            return (i1 * q + i2 + p) / 100;
        }

        private int Pass(int arg) => arg;

        private int ColorIntToGreyscale(int color)
        {
            int red = (color & 0x00ff0000) >> 16;
            int green = (color & 0x0000ff00) >> 8;
            int blue = (color & 0x000000ff) >> 8;
            int alpha = (color &  -0x10000000);

            int sum =((red * 30) + (green * 59) + (blue * 11))/ 100;

            return alpha | sum | sum << 8 | sum << 16;

        }

        private int Hash(string bookCode, bool selected, bool aura)
        {
            //Don't feel like using a tuple or something for the image, so i'm just using int keys for the dictionary
            //This should do a decent job of hashing it.
            int hash = bookCode.GetHashCode();

            if (selected) hash = unchecked (hash * 3);
            if (aura) hash = unchecked (hash * 17);

            return hash;
        }

        private Bitmap GetBaseImage(string bookCode)
        {
            return new Bitmap(Path.Combine(Application.StartupPath, "..", "..", "icons", "missing_book_temp.png"));
        }

        private int ColorToInt(Color color)
        {
            return (color.A << 24) | (color.R << 16) | (color.G << 8) | (color.B);
        }
    }
}