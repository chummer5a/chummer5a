using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;

namespace Chummer
{
    static class ImageExtensions
    {
        /// <summary>
        /// Converts a Base64 String into an Image.
        /// </summary>
        /// <param name="strBase64String">String to convert.</param>
        /// <returns>Image from the Base64 string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image ToImage(this string strBase64String)
        {
            Image imgReturn = null;
            byte[] bytImage = Convert.FromBase64String(strBase64String);
            if (bytImage.Length > 0)
            {
                using (MemoryStream objStream = new MemoryStream(bytImage, 0, bytImage.Length))
                {
                    objStream.Write(bytImage, 0, bytImage.Length);
                    imgReturn = Image.FromStream(objStream, true);
                }
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
            string strReturn = string.Empty;
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
                strReturn = Convert.ToBase64String(objImageStream.ToArray());
            }
            return strReturn;
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
            if (imgToConvert.PixelFormat == eNewFormat)
                return imgToConvert;

            Bitmap imgReturn = new Bitmap(imgToConvert.Width, imgToConvert.Height, eNewFormat);
            using (Graphics gr = Graphics.FromImage(imgReturn))
            {
                gr.DrawImage(imgToConvert, new Rectangle(0, 0, imgReturn.Width, imgReturn.Height));
            }
            return imgReturn;
        }
    }
}
