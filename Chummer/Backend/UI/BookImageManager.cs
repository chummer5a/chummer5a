using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using Chummer.UI.Options;

namespace Chummer.Backend.UI
{
    public class BookImageManager
    {
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

        Dictionary<int, Image> _cache = new Dictionary<int, Image>();
        private int _glowBorder = 20;

        public Func<float, float> Scale = d => d;
                    //d => 1 - ((1 - d) * (1 - d));
                    // d => d * d;

        public Image GetImage(string bookCode, bool selected, bool aura, int scale)
        {
            int hash = Hash(bookCode , selected , aura, scale);
            Image image;
            if (!_cache.TryGetValue(hash, out image))
            {
                image = GenerateImage(bookCode, selected, aura, scale);
                _cache.Add(hash, image);
            }

            return image;


        }

        private Lazy<Bitmap> CheckboxChecked = new Lazy<Bitmap>(() => (Bitmap)Properties.Resources.ResourceManager.GetObject("checkbox_checked"));
        private Lazy<Bitmap> CheckboxUnchecked = new Lazy<Bitmap>(() => (Bitmap)Properties.Resources.ResourceManager.GetObject("checkbox_unchecked"));

        private Image GenerateImage(string bookCode, bool enabled, bool aura, int scale)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Bitmap source = GetBaseImage(bookCode);

            //Comming soon: Bitmap checkbox (un)checked

            BitmapData sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            //This might create some _weird_ errors if somebody tries to run it on another endianess. ARM?
            int[] sourceArray = new int[sourceData.Width* sourceData.Height];

            int realWidth = (source.Width + GlowBorder * 2) / scale;
            int[] destinationArray = new int[realWidth * ((source.Height + GlowBorder * 2) / scale)];

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
            for (int y = 0; y < source.Height / scale; y++)
            {
                for (int x = 0; x < source.Width / scale; x++)
                {
                    int color = convert(sourceArray[(y * scale * source.Width ) + ( x * scale)]);
                    //Color preprocessing here
                    destinationArray[(y + (GlowBorder/ scale)) * realWidth + (GlowBorder/ scale) + x] = color;
                }
            }
            //Copy checkbox


            if (enabled)
            {
                Bitmap overlay = CheckboxChecked.Value;
                DrawOver(destinationArray, overlay, GlowBorder / scale, (source.Height - overlay.Height + GlowBorder) / scale, realWidth, scale);
            }
            else
            {
                Bitmap overlay = CheckboxUnchecked.Value;
                DrawOver(destinationArray, overlay, GlowBorder / scale, (source.Height - overlay.Height + GlowBorder) / scale, realWidth, scale);
            }

            //create aura
            if(aura)
                CreateAura(auracolor, backcolor, destinationArray, source.Width /scale, source.Height /scale, GlowBorder/scale);


            Bitmap final = new Bitmap((source.Width + GlowBorder * 2) / scale, (source.Height + GlowBorder * 2) / scale);
            BitmapData destinationData = final.LockBits(new Rectangle(0, 0, final.Width, final.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            Marshal.Copy(destinationArray, 0, destinationData.Scan0, destinationArray.Length);


            final.UnlockBits(destinationData);
            sw.TaskEnd($"Generation of image {bookCode}{(enabled ? "E" : "e")} {(aura ? "A" : "a")}");

            return final;
        }

        private void DrawOver(int[] destinationArray, Bitmap overlay, int x, int y, int width, int scale)
        {
            BitmapData sourceData = overlay.LockBits(new Rectangle(0, 0, overlay.Width, overlay.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            //This might create some _weird_ errors if somebody tries to run it on another endianess. ARM?
            int[] sourceArray = new int[sourceData.Width* sourceData.Height];
            Marshal.Copy(sourceData.Scan0, sourceArray, 0, Math.Abs(sourceData.Stride) * sourceData.Height / sizeof(int));
            overlay.UnlockBits(sourceData);

            int sourceWidth = overlay.Width;
            int sourceHeight = overlay.Height;

            int transpcolor = sourceArray[0];

            for (int yw = 0; yw < sourceHeight / scale; yw++)
            {
                for (int xw = 0; xw < sourceWidth / scale; xw++)
                {
                    int pixel = sourceArray[(sourceWidth * yw * scale) + (xw * scale)];
                    if (pixel != transpcolor)
                    {
                        destinationArray[width * (yw + y) + x + xw] = pixel;
                    }
                }
            }

        }

        private void CreateAura(int auracolor, int backcolor, int[] destinationArray, int sourceWidth, int sourceHeight, int glowSize)
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

        private int Pass(int arg)
        {
            return arg;
        }

        private int ColorIntToGreyscale(int color)
        {
            int red = (color & 0x00ff0000) >> 16;
            int green = (color & 0x0000ff00) >> 8;
            int blue = (color & 0x000000ff) >> 8;
            int alpha = (color &  -0x10000000);

            int sum =((red * 30) + (green * 59) + (blue * 11))/ 100;

            return alpha | sum | sum << 8 | sum << 16;

        }

        private int Hash(string bookCode, bool selected, bool aura, int scale)
        {
            //Don't feel like using a tuple or something for the image, so i'm just using int keys for the dictionary
            //This should do a decent job of hashing it.
            int hash = bookCode.GetHashCode();

            if (selected) hash = unchecked (hash * 3);
            if (aura) hash = unchecked (hash * 17);


            hash = unchecked (hash ^ scale * 19);
            return hash;
        }

        private Bitmap GetBaseImage(string bookCode)
        {
			string filePath = Path.Combine(Application.StartupPath, "images", $"{bookCode}.png");
			
			
			if (File.Exists(filePath))
			{
				Bitmap bmp2 = null;
				using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
				{
					//this construct is needed if we want to Close and Dispose the MemoryStream
					Bitmap bmpTmp = new Bitmap(fs);
					bmp2 = new Bitmap(bmpTmp);
					bmpTmp.Dispose();
					fs.Close();
				}

	            Console.WriteLine($"w{bmp2.Width} h{bmp2.Height}");
				return bmp2;
	        }
	        else
	        {
	            return (Bitmap) Properties.Resources.ResourceManager.GetObject("book/missing");
	        }
        }

        private int ColorToInt(Color color)
        {
            return (color.A << 24) | (color.R << 16) | (color.G << 8) | (color.B);
        }
    }
}