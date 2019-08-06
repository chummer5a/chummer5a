using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Chummer.Backend.UI
{
    public class BookImageManager
    {
        private const int HIGH_BYTE = 0xff << 24;


        public Color BackColor = SystemColors.Control;
        public Color GlowColor = Color.Blue;

        public BookImageManager()
        {
            if (Utils.IsLinux)
            {
                BackColor = Color.FromArgb(232, 232, 231);
            }
        }

        public int GlowBorder
        {
            get { return _glowBorder; }
            set
            {
                if(value < 0) throw new ArgumentOutOfRangeException();
                _glowBorder = value;
            }
        }

        private readonly ConcurrentDictionary<int, Image> _cache = new ConcurrentDictionary<int, Image>();
        private int _glowBorder = 20;

        public static Func<float, float> Scale = d => d;
                    //d => 1 - ((1 - d) * (1 - d));
                    // d => d * d;

        public Image GetImage(string bookCode, bool selected, bool aura, int scale)
        {
            //Optimistic concurrency. If image is not found, start generating it.
            int hash = Hash(bookCode , selected , aura, scale);
            Image image;

            if (!_cache.TryGetValue(hash, out image))
            {
                image = GenerateImage(bookCode, selected, aura, scale);
                if (!_cache.TryAdd(hash, image))
                {
                    //if we was outpaced, too bad. Dispose and get the winner
                    image.Dispose();
                    image = _cache[hash];
                }
            }

            return image;
        }


        private readonly Bitmap CheckboxChecked = Properties.Resources.checkbox_checked;
        private readonly Bitmap CheckboxUnchecked = Properties.Resources.checkbox_unchecked;
        private static readonly Bitmap MissingImage = Properties.Resources.missing_book;

        private Image GenerateImage(string bookCode, bool enabled, bool aura, int scale)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Bitmap source = GetBaseImage(bookCode);
            int source_w = source.Width, source_h = source.Height;

            //Comming soon: Bitmap checkbox (un)checked

            BitmapData sourceData = source.LockBits(new Rectangle(0, 0, source_w, source_h),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            //This might create some _weird_ errors if somebody tries to run it on another endianess. ARM?
            int[] sourceArray = new int[sourceData.Width* sourceData.Height];

            int realWidth = (source_w + GlowBorder * 2) / scale;
            int intRealHeight = (source_h + GlowBorder * 2) / scale;
            int[] destinationArray = new int[realWidth * intRealHeight];

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
            Bitmap overlay;
            if (enabled)
            {
                convert = Pass;
                overlay = CheckboxChecked;
            }
            else
            {
                convert = ColorIntToGreyscale;
                overlay = CheckboxUnchecked;
            }

            int intRealGlowSize = GlowBorder / scale;
            int intScaledSourceWidth = source_w / scale;
            int intScaledSourceHeight = source_h / scale;
            //Copy main image
            for (int y = 0; y < intScaledSourceHeight; y++)
            {
                for (int x = 0; x < intScaledSourceWidth; x++)
                {
                    int color = convert(sourceArray[(y * scale * source_w) + (x * scale)]);
                    //Color preprocessing here
                    destinationArray[(y + intRealGlowSize) * realWidth + intRealGlowSize + x] = color;
                }
            }
            //Copy checkbox
            DrawOverWithAlpha(destinationArray, overlay, intRealGlowSize, (source_h - overlay.Height + GlowBorder) / scale, realWidth, scale);

            //create aura
            if (aura)
                CreateAura(auracolor, backcolor, destinationArray, intScaledSourceWidth, intScaledSourceHeight, GlowBorder/scale);


            Bitmap final = new Bitmap(realWidth, intRealHeight);
            BitmapData destinationData = final.LockBits(new Rectangle(0, 0, final.Width, final.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            Marshal.Copy(destinationArray, 0, destinationData.Scan0, destinationArray.Length);


            final.UnlockBits(destinationData);
            sw.TaskEnd($"Generation of image {bookCode}{(enabled ? "E" : "e")} {(aura ? "A" : "a")}");

            return final;
        }

        private static void DrawOverWithAlpha(int[] destinationArray, Bitmap overlay, int x, int y, int width, int scale)
        {
            BitmapData sourceData = overlay.LockBits(new Rectangle(0, 0, overlay.Width, overlay.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            //This might create some _weird_ errors if somebody tries to run it on another endianess. ARM?
            int[] sourceArray = new int[sourceData.Width* sourceData.Height];
            Marshal.Copy(sourceData.Scan0, sourceArray, 0, Math.Abs(sourceData.Stride) * sourceData.Height / sizeof(int));
            overlay.UnlockBits(sourceData);

            int sourceWidth = overlay.Width;
            int sourceHeight = overlay.Height;
            int intScaledWidth = sourceWidth / scale;
            int intScaledHeight = sourceHeight / scale;

            for (int yw = 0; yw < intScaledHeight; yw++)
            {
                for (int xw = 0; xw < intScaledWidth; xw++)
                {
                    int pixel = sourceArray[(sourceWidth * yw * scale) + (xw * scale)];
                    
                    int memindex = width * (yw + y) + x + xw;

                    //If alpha byte is 255 just copy over, if 0, don't copy over, if 1-254 do the actual blend
                    if ((pixel & HIGH_BYTE) == HIGH_BYTE)
                    {
                        destinationArray[memindex] = pixel;
                    }
                    else if ((pixel & HIGH_BYTE) == 0)
                    {

                    }
                    else
                    {
                        int orginalColor = destinationArray[memindex];
                        int blend = ColorUtilities.ARGBIntXYZAlphaBlend(pixel, orginalColor);
                        destinationArray[memindex] = blend;
                    }
                }
            }

        }

        private static void CreateAura(int auracolor, int backcolor, int[] destinationArray, int sourceWidth, int sourceHeight, int glowSize)
        {
            int[] cachedColor = new int[glowSize];
            for (int i = 0; i < glowSize; i++)
            {
                cachedColor[i] = ColorUtilities.ARGBIntXYZInterpolate(auracolor, backcolor, Scale(i / (float) glowSize));
            }

            int realWidth = sourceWidth + glowSize * 2;
            int realHeight = sourceHeight + glowSize * 2;
            for (int y = 0; y < glowSize; y++)
            {
                int col = cachedColor[y];

                for (int x = 0; x < sourceWidth; x++)
                {
                    destinationArray[(y * realWidth) + glowSize + x] = col;
                }
            }

            for (int y = glowSize - 1; y >= 0; y--)
            {
                int col = cachedColor[y];
                for (int x = 0; x < sourceWidth; x++)
                {
                    destinationArray[((glowSize * 2 + sourceHeight - y - 1) * realWidth) + glowSize + x] = col;
                }
            }

            for (int y = 0; y < sourceHeight; y++)
            {
                for (int x = 0; x < glowSize; x++)
                {
                    int col = cachedColor[x];
                    destinationArray[((y + glowSize) * realWidth) + x] = col;
                }

                for (int x = 0; x < glowSize; x++)
                {
                    int col = cachedColor[glowSize - x - 1];
                    destinationArray[((y + glowSize) * realWidth) + sourceWidth + glowSize + x] = col;
                }
            }

            for(int y = 0; y < glowSize; y++)
                for (int x = 0; x < glowSize; x++)
                {
                    int x2 = (glowSize - x);
                    int y2 = (glowSize - y);
                    float distance = (float)Math.Sqrt(x2 * x2 + y2 * y2 );
                    int col = ColorUtilities.ARGBIntXYZInterpolate(auracolor, backcolor, Scale(Math.Max(0,1 -(distance / glowSize))));
                    destinationArray[(y * realWidth) + x] = col;
                    destinationArray[(y * realWidth) + realWidth - x - 1] = col;
                    destinationArray[((realHeight - y - 1) * realWidth) + x] = col;
                    destinationArray[((realHeight - y - 1) * realWidth) + realWidth - x - 1] = col;
                }
        }

        private static int IntColorInterpolate(int color1, int color2, float scale)
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

        private static int IntInterpolate(int i1, int i2, float scale)
        {
            int q = (int) (scale * 100);
            int p = 100 - q;

            return (i1 * q + i2 + p) / 100;
        }

        private static int Pass(int arg)
        {
            return arg;
        }

        private static int ColorIntToGreyscale(int color)
        {
            int red = (color & 0x00ff0000) >> 16;
            int green = (color & 0x0000ff00) >> 8;
            int blue = (color & 0x000000ff);
            int alpha = (color &  -0x10000000);

            int sum =((red * 30) + (green * 59) + (blue * 11))/ 100;

            return alpha | sum | sum << 8 | sum << 16;
        }

        private static int Hash(string bookCode, bool selected, bool aura, int scale)
        {
            //Don't feel like using a tuple or something for the image, so i'm just using int keys for the dictionary
            //This should do a decent job of hashing it.
            int hash = bookCode.GetHashCode();

            if (selected) hash = unchecked (hash * 3);
            if (aura) hash = unchecked (hash * 17);


            hash = unchecked (hash ^ scale * 19);
            return hash;
        }

        private static ConcurrentDictionary<string, Bitmap> imageCache = new ConcurrentDictionary<string, Bitmap>();
        private static Bitmap GetBaseImage(string bookCode)
        {
            return imageCache.GetOrAdd(bookCode, LoadBookImage);
        }

        private static Bitmap LoadBookImage(string bookCode)
        {
            string filePath = Path.Combine(Application.StartupPath, "images", $"{bookCode}.png");
            return File.Exists(filePath) ? new Bitmap(filePath) : MissingImage;
        }

        private static int ColorToInt(Color color)
        {
            return (color.A << 24) | (color.R << 16) | (color.G << 8) | (color.B);
        }
    }
}
