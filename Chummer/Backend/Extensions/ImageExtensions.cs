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
        /// <param name="eFormat">Pixel format in which the Bitmap is returned.</param>
        /// <returns>Image from the Base64 string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitmap ToImage(this string strBase64String, PixelFormat eFormat)
        {
            using (Image imgInput = strBase64String.ToImage())
            {
                Bitmap bmpInput = new Bitmap(imgInput);
                if (bmpInput.PixelFormat == eFormat)
                    return bmpInput;
                try
                {
                    return bmpInput.ConvertPixelFormat(eFormat);
                }
                finally
                {
                    bmpInput.Dispose();
                }
            }
        }

        /// <summary>
        /// Converts an Image into a Base64 string.
        /// </summary>
        /// <param name="imgToConvert">Image to convert.</param>
        /// <param name="eOverrideFormat">The image format in which the image should be saved. If null, will use <paramref name="imgToConvert"/>'s RawFormat.</param>
        /// <returns>Base64 string from Image.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToBase64String(this Image imgToConvert, ImageFormat eOverrideFormat = null)
        {
            if (imgToConvert == null)
                return string.Empty;
            using (MemoryStream objImageStream = new MemoryStream())
            {
                imgToConvert.Save(objImageStream, eOverrideFormat ?? imgToConvert.RawFormat);
                return Convert.ToBase64String(objImageStream.ToArray());
            }
        }

        /// <summary>
        /// Converts an Image into a Base64 string.
        /// </summary>
        /// <param name="imgToConvert">Image to convert.</param>
        /// <param name="objCodecInfo">Encoder to use to encode the image.</param>
        /// <param name="lstEncoderParameters">List of parameters for <paramref name="objCodecInfo"/>.</param>
        /// <returns>Base64 string from Image.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToBase64String(this Image imgToConvert, ImageCodecInfo objCodecInfo, EncoderParameters lstEncoderParameters)
        {
            if (imgToConvert == null)
                return string.Empty;
            using (MemoryStream objImageStream = new MemoryStream())
            {
                imgToConvert.Save(objImageStream, objCodecInfo, lstEncoderParameters);
                return Convert.ToBase64String(objImageStream.ToArray());
            }
        }

        /// <summary>
        /// Converts an Image into a Base64 string of its Jpeg version with a custom quality setting (default ImageFormat.Jpeg quality is 50).
        /// </summary>
        /// <param name="imgToConvert">Image to convert.</param>
        /// <param name="intQuality">Jpeg quality to use. 90 by default.</param>
        /// <returns>Base64 string of Jpeg version of Image with a quality of <paramref name="intQuality"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToBase64StringAsJpeg(this Image imgToConvert, int intQuality = 90)
        {
            if (imgToConvert == null)
                return string.Empty;
            ImageCodecInfo objJpegEncoder = GetEncoder(ImageFormat.Jpeg);
            EncoderParameters lstJpegParameters = new EncoderParameters(1)
            {
                Param = {[0] = new EncoderParameter(Encoder.Quality, Math.Min(Math.Max(intQuality, 0), 100)) }
            };
            return imgToConvert.ToBase64String(objJpegEncoder, lstJpegParameters);
        }

        /// <summary>
        /// Converts a Bitmap into a new one with a different PixelFormat. (PixelFormat.Format32bppPArgb draws the fastest)
        /// </summary>
        /// <param name="bmpToConvert">Bitmap to convert.</param>
        /// <param name="eNewFormat">New format to which to convert.</param>
        /// <returns>Bitmap in the new format.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitmap ConvertPixelFormat(this Bitmap bmpToConvert, PixelFormat eNewFormat)
        {
            return bmpToConvert?.Clone(new Rectangle(0, 0, bmpToConvert.Width, bmpToConvert.Height), eNewFormat);
        }

        /// <summary>
        /// Returns the encoder of an image format
        /// </summary>
        /// <param name="eFormat">Image format whose encoder is to be fetched</param>
        /// <returns>The encoder of <paramref name="eFormat"/> if one is found, otherwise null.</returns>
        public static ImageCodecInfo GetEncoder(this ImageFormat eFormat)
        {
            foreach (ImageCodecInfo objCodec in ImageCodecInfo.GetImageDecoders())
            {
                if (objCodec.FormatID == eFormat.Guid)
                {
                    return objCodec;
                }
            }
            return null;
        }
    }
}
