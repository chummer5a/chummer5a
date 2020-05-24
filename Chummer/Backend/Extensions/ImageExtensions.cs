/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using NLog;

namespace Chummer
{
    public static class ImageExtensions
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Converts a Base64 String into an Image.
        /// </summary>
        /// <param name="strBase64String">String to convert.</param>
        /// <returns>Image from the Base64 string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image ToImage(this string strBase64String)
        {
            Image imgReturn = null;
            try
            {
                byte[] bytImage = Convert.FromBase64String(strBase64String);
                if (bytImage.Length > 0)
                {
                    using (MemoryStream objStream = new MemoryStream(bytImage, 0, bytImage.Length))
                    {
                        objStream.Write(bytImage, 0, bytImage.Length);
                        imgReturn = Image.FromStream(objStream, true);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warn(e);
            }

            return imgReturn;
        }

        /// <summary>
        /// Converts a Base64 String into a Bitmap with a specific format.
        /// </summary>
        /// <param name="strBase64String">String to convert.</param>
        /// <param name="objFormat">Pixel format in which the Bitmap is returned.</param>
        /// <returns>Image from the Base64 string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitmap ToImage(this string strBase64String, PixelFormat objFormat)
        {
            return (new Bitmap(strBase64String.ToImage())).ConvertPixelFormat(objFormat);
        }

        /// <summary>
        /// Converts an Image into a Base64 string.
        /// </summary>
        /// <param name="imgToConvert">Image to convert.</param>
        /// <returns>Base64 string from Image.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToBase64String(this Image imgToConvert)
        {
            if (imgToConvert == null)
                return string.Empty;
            using (MemoryStream objImageStream = new MemoryStream())
            {
                try
                {
                    imgToConvert.Save(objImageStream, imgToConvert.RawFormat);
                }
                catch (ArgumentNullException)
                {
                    imgToConvert.Save(objImageStream, ImageFormat.Png);
                }

                return Convert.ToBase64String(objImageStream.ToArray());
            }
        }

        /// <summary>
        /// Converts a Bitmap into a new one with a different PixelFormat. (PixelFormat.Format32bppPArgb draws the fastest)
        /// </summary>
        /// <param name="imgToConvert">Bitmap to convert.</param>
        /// <param name="eNewFormat">New format to which to convert.</param>
        /// <returns>Bitmap in the new format.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitmap ConvertPixelFormat(this Bitmap imgToConvert, PixelFormat eNewFormat)
        {
            if (imgToConvert == null || imgToConvert.PixelFormat == eNewFormat)
                return imgToConvert;

            Bitmap imgReturn = new Bitmap(imgToConvert.Width, imgToConvert.Height, eNewFormat);
            using (Graphics gr = Graphics.FromImage(imgReturn))
                gr.DrawImage(imgToConvert, new Rectangle(0, 0, imgReturn.Width, imgReturn.Height));
            return imgReturn;
        }
    }
}
