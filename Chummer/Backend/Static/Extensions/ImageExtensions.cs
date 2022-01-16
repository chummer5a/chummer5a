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
using System.Threading.Tasks;
using NLog;

namespace Chummer
{
    public static class ImageExtensions
    {
        private static readonly Lazy<ImageCodecInfo> s_LzyJpegEncoder =
            new Lazy<ImageCodecInfo>(() => GetEncoder(ImageFormat.Jpeg));

        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Takes a Base64 String that is meant to represent an Image and turns it into a Base64 String that is meant to represent a JPEG
        /// </summary>
        /// <param name="strBase64String">String representing image to compress.</param>
        /// <param name="intQuality">Jpeg quality to use. Default is -1, which automatically sets quality based on image size down to 50 at worst (larger images get lower quality).</param>
        /// <returns>String of compressed image.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string CompressBase64String(this string strBase64String, int intQuality = -1)
        {
            if (string.IsNullOrEmpty(strBase64String))
                return string.Empty;
            using (Image imgTemp = strBase64String.ToImage())
            {
                return imgTemp == null ? strBase64String : imgTemp.ToBase64StringAsJpeg(intQuality);
            }
        }

        /// <summary>
        /// Takes a Base64 String that is meant to represent an Image and turns it into a Base64 String that is meant to represent a JPEG
        /// </summary>
        /// <param name="strBase64String">String representing image to compress.</param>
        /// <param name="intQuality">Jpeg quality to use. Default is -1, which automatically sets quality based on image size down to 50 at worst (larger images get lower quality).</param>
        /// <returns>String of compressed image.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<string> CompressBase64StringAsync(this string strBase64String, int intQuality = -1)
        {
            return string.IsNullOrEmpty(strBase64String) ? new Task<string>(() => string.Empty) : GetImage();
            // Split into a private method for performance reasons
            async Task<string> GetImage()
            {
                using (Image imgTemp = await strBase64String.ToImageAsync())
                {
                    return imgTemp == null ? strBase64String : await imgTemp.ToBase64StringAsJpegAsync(intQuality);
                }
            }
        }

        /// <summary>
        /// Syntactic sugar to generate a thumbnail for an image that is also compressed as a JPEG.
        /// </summary>
        /// <param name="imgToConvert">Image whose thumbnail is to be generated.</param>
        /// <param name="intThumbWidth">Width of the thumbnail.</param>
        /// <param name="intThumbHeight">Height of the thumbnail.</param>
        /// <param name="blnKeepAspectRatio">Whether or not to make sure we retain the aspect ratio of the old image.</param>
        /// <param name="intQuality">Jpeg quality to use. Default is -1, which automatically sets quality based on image size down to 50 at worst (larger images get lower quality).</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image GetCompressedThumbnailImage(this Image imgToConvert, int intThumbWidth, int intThumbHeight,
            bool blnKeepAspectRatio = true, int intQuality = -1)
        {
            if (imgToConvert == null)
                return null;
            int intImageWidth = imgToConvert.Width;
            int intImageHeight = imgToConvert.Height;
            if (blnKeepAspectRatio)
            {
                double dblImageAspectRatio = intImageWidth / (double)intImageHeight;
                double dblThumbAspectRatio = intThumbWidth / (double)intThumbHeight;
                // Too wide, use height as correct measure
                if (dblThumbAspectRatio > dblImageAspectRatio)
                {
                    intThumbWidth = (intThumbHeight * dblImageAspectRatio).StandardRound();
                }
                // Too tall, use width as correct measure
                else if (dblThumbAspectRatio < dblImageAspectRatio)
                {
                    intThumbHeight = (intThumbWidth / dblImageAspectRatio).StandardRound();
                }
            }

            if (intThumbWidth >= intImageWidth && intThumbHeight >= intImageHeight)
            {
                return imgToConvert.GetCompressedImage(intQuality);
            }

            using (Image imgThumbnail = imgToConvert.GetThumbnailImage(intThumbWidth, intThumbHeight, null, IntPtr.Zero))
            {
                return imgThumbnail.GetCompressedImage(intQuality);
            }
        }

        /// <summary>
        /// Syntactic sugar to generate a thumbnail for an image that is also compressed as a JPEG.
        /// </summary>
        /// <param name="imgToConvert">Image whose thumbnail is to be generated.</param>
        /// <param name="intThumbWidth">Width of the thumbnail.</param>
        /// <param name="intThumbHeight">Height of the thumbnail.</param>
        /// <param name="blnKeepAspectRatio">Whether or not to make sure we retain the aspect ratio of the old image.</param>
        /// <param name="intQuality">Jpeg quality to use. Default is -1, which automatically sets quality based on image size down to 50 at worst (larger images get lower quality).</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<Image> GetCompressedThumbnailImageAsync(this Image imgToConvert, int intThumbWidth, int intThumbHeight, bool blnKeepAspectRatio = true, int intQuality = -1)
        {
            if (imgToConvert == null)
                return new Task<Image>(() => null);
            int intImageWidth = imgToConvert.Width;
            int intImageHeight = imgToConvert.Height;
            if (blnKeepAspectRatio)
            {
                double dblImageAspectRatio = intImageWidth / (double)intImageHeight;
                double dblThumbAspectRatio = intThumbWidth / (double)intThumbHeight;
                // Too wide, use height as correct measure
                if (dblThumbAspectRatio > dblImageAspectRatio)
                {
                    intThumbWidth = (intThumbHeight * dblImageAspectRatio).StandardRound();
                }
                // Too tall, use width as correct measure
                else if (dblThumbAspectRatio < dblImageAspectRatio)
                {
                    intThumbHeight = (intThumbWidth / dblImageAspectRatio).StandardRound();
                }
            }

            return GetImage();

            // Split into a private method for performance reasons
            async Task<Image> GetImage()
            {
                if (intThumbWidth >= intImageWidth && intThumbHeight >= intImageHeight)
                {
                    return await imgToConvert.GetCompressedImageAsync(intQuality);
                }

                using (Image objThumbnail = imgToConvert.GetThumbnailImage(intThumbWidth, intThumbHeight, null, IntPtr.Zero))
                {
                    return await objThumbnail.GetCompressedImageAsync(intQuality);
                }
            }
        }

        /// <summary>
        /// Get a clone of an image that is compressed as a Jpeg.
        /// </summary>
        /// <param name="imgToConvert">Image to convert.</param>
        /// <param name="intQuality">Jpeg quality to use. Default is -1, which automatically sets quality based on image size down to 50 at worst (larger images get lower quality).</param>
        /// <returns>A clone of <paramref name="imgToConvert"/> that is compressed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Image GetCompressedImage(this Image imgToConvert, int intQuality = -1)
        {
            if (imgToConvert == null)
                return null;
            EncoderParameters lstJpegParameters = new EncoderParameters(1)
            {
                Param = { [0] = new EncoderParameter(Encoder.Quality, ProcessJpegQualitySetting(imgToConvert, intQuality)) }
            };
            // We need to clone the image before saving it because of weird GDI+ errors that can happen if we don't
            using (Bitmap bmpClone = new Bitmap(imgToConvert))
            {
                using (MemoryStream objImageStream = new MemoryStream())
                {
                    bmpClone.Save(objImageStream, s_LzyJpegEncoder.Value, lstJpegParameters);
                    objImageStream.Position = 0;
                    return Image.FromStream(objImageStream, true);
                }
            }
        }

        /// <summary>
        /// Get a clone of an image that is compressed as a Jpeg.
        /// </summary>
        /// <param name="imgToConvert">Image to convert.</param>
        /// <param name="intQuality">Jpeg quality to use. Default is -1, which automatically sets quality based on image size down to 50 at worst (larger images get lower quality).</param>
        /// <returns>A clone of <paramref name="imgToConvert"/> that is compressed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<Image> GetCompressedImageAsync(this Image imgToConvert, int intQuality = -1)
        {
            if (imgToConvert == null)
                return new Task<Image>(() => null);
            EncoderParameters lstJpegParameters = new EncoderParameters(1)
            {
                Param = { [0] = new EncoderParameter(Encoder.Quality, ProcessJpegQualitySetting(imgToConvert, intQuality)) }
            };
            return Task.Run(() =>
            {
                // We need to clone the image before saving it because of weird GDI+ errors that can happen if we don't
                using (Bitmap bmpClone = new Bitmap(imgToConvert))
                {
                    using (MemoryStream objImageStream = new MemoryStream())
                    {
                        bmpClone.Save(objImageStream, s_LzyJpegEncoder.Value, lstJpegParameters);
                        objImageStream.Position = 0;
                        return Image.FromStream(objImageStream, true);
                    }
                }
            });
        }

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
                byte[] achrImage = Convert.FromBase64String(strBase64String);
                if (achrImage.Length > 0)
                {
                    using (MemoryStream objStream = new MemoryStream(achrImage, 0, achrImage.Length))
                    {
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
        /// Converts a Base64 String into an Image.
        /// </summary>
        /// <param name="strBase64String">String to convert.</param>
        /// <returns>Image from the Base64 string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<Image> ToImageAsync(this string strBase64String)
        {
            Image imgReturn = null;
            try
            {
                byte[] achrImage = Convert.FromBase64String(strBase64String);
                if (achrImage.Length > 0)
                {
                    using (MemoryStream objStream = new MemoryStream())
                    {
                        await objStream.WriteAsync(achrImage, 0, achrImage.Length);
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
        public static async Task<Bitmap> ToImageAsync(this string strBase64String, PixelFormat eFormat)
        {
            using (Image imgInput = await strBase64String.ToImageAsync())
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
            // We need to clone the image before saving it because of weird GDI+ errors that can happen if we don't
            using (Bitmap bmpClone = new Bitmap(imgToConvert))
            {
                using (MemoryStream objImageStream = new MemoryStream())
                {
                    if (eOverrideFormat == null)
                    {
                        // Need to do this because calling RawFormat on its own will result in the system not finding its encoder
                        if (Equals(imgToConvert.RawFormat, ImageFormat.Jpeg))
                            eOverrideFormat = ImageFormat.Jpeg;
                        else if (Equals(imgToConvert.RawFormat, ImageFormat.Gif))
                            eOverrideFormat = ImageFormat.Gif;
                        else
                            eOverrideFormat = ImageFormat.Png;
                    }

                    bmpClone.Save(objImageStream, eOverrideFormat);
                    return Convert.ToBase64String(objImageStream.ToArray());
                }
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
            // We need to clone the image before saving it because of weird GDI+ errors that can happen if we don't
            using (Bitmap bmpClone = new Bitmap(imgToConvert))
            {
                using (MemoryStream objImageStream = new MemoryStream())
                {
                    bmpClone.Save(objImageStream, objCodecInfo, lstEncoderParameters);
                    return Convert.ToBase64String(objImageStream.ToArray());
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
        public static Task<string> ToBase64StringAsync(this Image imgToConvert, ImageFormat eOverrideFormat = null)
        {
            if (imgToConvert == null)
                return new Task<string>(() => string.Empty);
            return Task.Run(() =>
            {
                // We need to clone the image before saving it because of weird GDI+ errors that can happen if we don't
                using (Bitmap bmpClone = new Bitmap(imgToConvert))
                {
                    using (MemoryStream objImageStream = new MemoryStream())
                    {
                        if (eOverrideFormat == null)
                        {
                            // Need to do this because calling RawFormat on its own will result in the system not finding its encoder
                            if (Equals(imgToConvert.RawFormat, ImageFormat.Jpeg))
                                eOverrideFormat = ImageFormat.Jpeg;
                            else if (Equals(imgToConvert.RawFormat, ImageFormat.Gif))
                                eOverrideFormat = ImageFormat.Gif;
                            else
                                eOverrideFormat = ImageFormat.Png;
                        }

                        bmpClone.Save(objImageStream, eOverrideFormat);
                        return Convert.ToBase64String(objImageStream.ToArray());
                    }
                }
            });
        }

        /// <summary>
        /// Converts an Image into a Base64 string.
        /// </summary>
        /// <param name="imgToConvert">Image to convert.</param>
        /// <param name="objCodecInfo">Encoder to use to encode the image.</param>
        /// <param name="lstEncoderParameters">List of parameters for <paramref name="objCodecInfo"/>.</param>
        /// <returns>Base64 string from Image.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<string> ToBase64StringAsync(this Image imgToConvert, ImageCodecInfo objCodecInfo, EncoderParameters lstEncoderParameters)
        {
            if (imgToConvert == null)
                return new Task<string>(() => string.Empty);
            return Task.Run(() =>
            {
                // We need to clone the image before saving it because of weird GDI+ errors that can happen if we don't
                using (Bitmap bmpClone = new Bitmap(imgToConvert))
                {
                    using (MemoryStream objImageStream = new MemoryStream())
                    {
                        bmpClone.Save(objImageStream, objCodecInfo, lstEncoderParameters);
                        return Convert.ToBase64String(objImageStream.ToArray());
                    }
                }
            });
        }

        /// <summary>
        /// Converts an Image into a Base64 string of its Jpeg version with a custom quality setting (default ImageFormat.Jpeg quality is 50).
        /// </summary>
        /// <param name="imgToConvert">Image to convert.</param>
        /// <param name="intQuality">Jpeg quality to use. Default is -1, which automatically sets quality based on image size down to 50 at worst (larger images get lower quality).</param>
        /// <returns>Base64 string of Jpeg version of Image with a quality of <paramref name="intQuality"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToBase64StringAsJpeg(this Image imgToConvert, int intQuality = -1)
        {
            if (imgToConvert == null)
                return string.Empty;
            EncoderParameters lstJpegParameters = new EncoderParameters(1)
            {
                Param = { [0] = new EncoderParameter(Encoder.Quality, ProcessJpegQualitySetting(imgToConvert, intQuality)) }
            };
            return imgToConvert.ToBase64String(s_LzyJpegEncoder.Value, lstJpegParameters);
        }

        /// <summary>
        /// Converts an Image into a Base64 string of its Jpeg version with a custom quality setting (default ImageFormat.Jpeg quality is 50).
        /// </summary>
        /// <param name="imgToConvert">Image to convert.</param>
        /// <param name="intQuality">Jpeg quality to use. Default is -1, which automatically sets quality based on image size down to 50 at worst (larger images get lower quality).</param>
        /// <returns>Base64 string of Jpeg version of Image with a quality of <paramref name="intQuality"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<string> ToBase64StringAsJpegAsync(this Image imgToConvert, int intQuality = -1)
        {
            if (imgToConvert == null)
                return new Task<string>(() => string.Empty);
            EncoderParameters lstJpegParameters = new EncoderParameters(1)
            {
                Param = { [0] = new EncoderParameter(Encoder.Quality, ProcessJpegQualitySetting(imgToConvert, intQuality)) }
            };
            return imgToConvert.ToBase64StringAsync(s_LzyJpegEncoder.Value, lstJpegParameters);
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
            Guid objTargetGuid = eFormat.Guid;
            return Array.Find(ImageCodecInfo.GetImageDecoders(), objCodec => objCodec.FormatID == objTargetGuid);
        }

        /// <summary>
        /// Process Jpeg quality settings in a way that's uniform across all methods that export an image to a Jpeg.
        /// </summary>
        /// <param name="imgToConvert">Image for which the Jpeg will be generated.</param>
        /// <param name="intQuality">Jpeg quality to use. Values less than 0 are treated as "automatic" where quality is adjusted between 50 and 100 depending on image size.</param>
        /// <returns>A valid Jpeg quality setting for <paramref name="imgToConvert"/>.</returns>
        private static int ProcessJpegQualitySetting(this Image imgToConvert, int intQuality)
        {
            if (intQuality >= 0)
                return intQuality > 100 ? 100 : intQuality;
            double dblPixelCount = imgToConvert.Height;
            dblPixelCount *= imgToConvert.Width;
            // Jpeg encoding works in 8x8 blocks, so we should ideally scale based on a logarithm based on a power of 2 (2^10 in this case)
            return dblPixelCount <= 1024
                ? 100
                : (50.0 * (1.0 + 2 * 0.5.RaiseToPower(Math.Log(dblPixelCount, 1024.0)))).StandardRound(); // Sub-50 quality gets iffy for a lot of stuff, so just scale between 50 and 100 to be on the safe side
        }
    }
}
