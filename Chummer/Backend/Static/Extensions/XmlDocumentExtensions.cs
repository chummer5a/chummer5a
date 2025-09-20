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
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using Microsoft.IO;

namespace Chummer
{
    public static class XmlDocumentExtensions
    {
        /// <summary>
        /// Syntactic sugar for calling <see cref="XmlDocument.Load(string)"/> with standard encoding and XmlReader settings
        /// </summary>
        /// <param name="xmlDocument">The document into which the XML data should be loaded.</param>
        /// <param name="strFileName">The file to use.</param>
        /// <param name="blnSafe">Whether to check characters for validity while loading.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadStandard(this XmlDocument xmlDocument, string strFileName, bool blnSafe = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (FileStream objFileStream = new FileStream(strFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                token.ThrowIfCancellationRequested();
                using (StreamReader objStreamReader = new StreamReader(objFileStream, Encoding.UTF8, true))
                {
                    token.ThrowIfCancellationRequested();
                    using (XmlReader objReader = XmlReader.Create(objStreamReader,
                               blnSafe ? GlobalSettings.SafeXmlReaderSettings : GlobalSettings.UnSafeXmlReaderSettings))
                    {
                        token.ThrowIfCancellationRequested();
                        xmlDocument.Load(objReader);
                    }
                }
            }
        }

        /// <summary>
        /// Syntactic sugar for asynchronously calling <see cref="XmlDocument.Load(string)"/> with standard encoding and XmlReader settings
        /// </summary>
        /// <param name="xmlDocument">The document into which the XML data should be loaded.</param>
        /// <param name="strFileName">The file to use.</param>
        /// <param name="blnSafe">Whether to check characters for validity while loading.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task LoadStandardAsync(this XmlDocument xmlDocument, string strFileName, bool blnSafe = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (FileStream objFileStream
                   = new FileStream(strFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                token.ThrowIfCancellationRequested();
                using (StreamReader objStreamReader = new StreamReader(objFileStream, Encoding.UTF8, true))
                {
                    token.ThrowIfCancellationRequested();
                    using (XmlReader objReader = XmlReader.Create(objStreamReader,
                               blnSafe
                                   ? GlobalSettings.SafeXmlReaderSettings
                                   : GlobalSettings.UnSafeXmlReaderSettings))
                    {
                        token.ThrowIfCancellationRequested();
                        // ReSharper disable once AccessToDisposedClosure
                        await TaskExtensions.RunWithoutEC(() => xmlDocument.Load(objReader), token).ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// Syntactic sugar for calling <see cref="XmlDocument.Load(string)"/> on an LZMA-compressed file with standard encoding and XmlReader settings
        /// </summary>
        /// <param name="xmlDocument">The document into which the XML data should be loaded.</param>
        /// <param name="strFileName">The file to use.</param>
        /// <param name="blnSafe">Whether to check characters for validity while loading.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadStandardFromLzmaCompressed(this XmlDocument xmlDocument, string strFileName, bool blnSafe = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (FileStream objFileStream = new FileStream(strFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                token.ThrowIfCancellationRequested();
                using (RecyclableMemoryStream objMemoryStream = new RecyclableMemoryStream(Utils.MemoryStreamManager, "LzmaMemoryStream", (int)objFileStream.Length))
                {
                    token.ThrowIfCancellationRequested();
                    objFileStream.DecompressLzmaFile(objMemoryStream);
                    token.ThrowIfCancellationRequested();
                    objMemoryStream.Seek(0, SeekOrigin.Begin);
                    token.ThrowIfCancellationRequested();
                    using (StreamReader objStreamReader
                           = new StreamReader(objMemoryStream, Encoding.UTF8, true))
                    {
                        token.ThrowIfCancellationRequested();
                        using (XmlReader objReader = XmlReader.Create(objStreamReader,
                                   blnSafe
                                       ? GlobalSettings.SafeXmlReaderSettings
                                       : GlobalSettings.UnSafeXmlReaderSettings))
                        {
                            token.ThrowIfCancellationRequested();
                            xmlDocument.Load(objReader);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Syntactic sugar for asynchronously calling <see cref="XmlDocument.Load(string)"/> on an LZMA-compressed file with standard encoding and XmlReader settings
        /// </summary>
        /// <param name="xmlDocument">The document into which the XML data should be loaded.</param>
        /// <param name="strFileName">The file to use.</param>
        /// <param name="blnSafe">Whether to check characters for validity while loading.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task LoadStandardFromLzmaCompressedAsync(this XmlDocument xmlDocument, string strFileName, bool blnSafe = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (FileStream objFileStream = new FileStream(strFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                token.ThrowIfCancellationRequested();
                using (RecyclableMemoryStream objMemoryStream = new RecyclableMemoryStream(Utils.MemoryStreamManager, "LzmaMemoryStream", (int)objFileStream.Length))
                {
                    await objFileStream.DecompressLzmaFileAsync(objMemoryStream, token: token).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    objMemoryStream.Seek(0, SeekOrigin.Begin);
                    token.ThrowIfCancellationRequested();
                    using (StreamReader objStreamReader
                           = new StreamReader(objMemoryStream, Encoding.UTF8, true))
                    {
                        token.ThrowIfCancellationRequested();
                        using (XmlReader objReader = XmlReader.Create(objStreamReader,
                                   blnSafe
                                       ? GlobalSettings.SafeXmlReaderSettings
                                       : GlobalSettings.UnSafeXmlReaderSettings))
                        {
                            token.ThrowIfCancellationRequested();
                            // ReSharper disable once AccessToDisposedClosure
                            await TaskExtensions.RunWithoutEC(() => xmlDocument.Load(objReader), token).ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Syntactic sugar for calling <see cref="XmlDocument.Load(string)"/> with standard encoding and XmlReader settings in a way that does not immediately except out if the file is temporarily unavailable
        /// </summary>
        /// <param name="xmlDocument">The document into which the XML data should be loaded.</param>
        /// <param name="strFileName">The file to use.</param>
        /// <param name="blnSafe">Whether to check characters for validity while loading.</param>
        /// <param name="intTimeout">Maximum amount of time to wait in case a file is in use, in milliseconds.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadStandardPatient(this XmlDocument xmlDocument, string strFileName, bool blnSafe = true, int intTimeout = Utils.SleepEmergencyReleaseMaxTicks, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            bool blnSuccess = false;
            for (int i = 0; i < intTimeout; i += Utils.DefaultSleepDuration)
            {
                try
                {
                    xmlDocument.LoadStandard(strFileName, blnSafe, token);
                    blnSuccess = true;
                    break;
                }
                catch (IOException)
                {
                    // swallow this unless we are at the emergency release stage
                    if (i >= intTimeout - Utils.DefaultSleepDuration)
                        throw;
                }

                Utils.SafeSleep(token);
            }

            if (!blnSuccess)
                throw new TimeoutException();
        }

        /// <summary>
        /// Syntactic sugar for asynchronously calling <see cref="XmlDocument.Load(string)"/> with standard encoding and XmlReader settings in a way that does not immediately except out if the file is temporarily unavailable
        /// </summary>
        /// <param name="xmlDocument">The document into which the XML data should be loaded.</param>
        /// <param name="strFileName">The file to use.</param>
        /// <param name="blnSafe">Whether to check characters for validity while loading.</param>
        /// <param name="intTimeout">Maximum amount of time to wait in case a file is in use, in milliseconds.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task LoadStandardPatientAsync(this XmlDocument xmlDocument, string strFileName, bool blnSafe = true, int intTimeout = Utils.SleepEmergencyReleaseMaxTicks, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            bool blnSuccess = false;
            for (int i = 0; i < intTimeout; i += Utils.DefaultSleepDuration)
            {
                try
                {
                    await xmlDocument.LoadStandardAsync(strFileName, blnSafe, token).ConfigureAwait(false);
                    blnSuccess = true;
                    break;
                }
                catch (IOException)
                {
                    // swallow this unless we are at the emergency release stage
                    if (i >= intTimeout - Utils.DefaultSleepDuration)
                        throw;
                }

                await Utils.SafeSleepAsync(token).ConfigureAwait(false);
            }

            if (!blnSuccess)
                throw new TimeoutException();
        }

        /// <summary>
        /// Syntactic sugar for calling <see cref="XmlDocument.Load(string)"/> on an LZMA-compressed file with standard encoding and XmlReader settings in a way that does not immediately except out if the file is temporarily unavailable
        /// </summary>
        /// <param name="xmlDocument">The document into which the XML data should be loaded.</param>
        /// <param name="strFileName">The file to use.</param>
        /// <param name="blnSafe">Whether to check characters for validity while loading.</param>
        /// <param name="intTimeout">Maximum amount of time to wait in case a file is in use, in milliseconds.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadStandardFromLzmaCompressedPatient(this XmlDocument xmlDocument, string strFileName, bool blnSafe = true, int intTimeout = Utils.SleepEmergencyReleaseMaxTicks, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            bool blnSuccess = false;
            for (int i = 0; i < intTimeout; i += Utils.DefaultSleepDuration)
            {
                try
                {
                    xmlDocument.LoadStandardFromLzmaCompressed(strFileName, blnSafe, token);
                    blnSuccess = true;
                    break;
                }
                catch (IOException)
                {
                    // swallow this unless we are at the emergency release stage
                    if (i >= intTimeout - Utils.DefaultSleepDuration)
                        throw;
                }

                Utils.SafeSleep(token);
            }

            if (!blnSuccess)
                throw new TimeoutException();
        }

        /// <summary>
        /// Syntactic sugar for asynchronously calling <see cref="XmlDocument.Load(string)"/> on an LZMA-compressed file with standard encoding and XmlReader settings in a way that does not immediately except out if the file is temporarily unavailable
        /// </summary>
        /// <param name="xmlDocument">The document into which the XML data should be loaded.</param>
        /// <param name="strFileName">The file to use.</param>
        /// <param name="blnSafe">Whether to check characters for validity while loading.</param>
        /// <param name="intTimeout">Maximum amount of time to wait in case a file is in use, in milliseconds.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task LoadStandardFromLzmaCompressedPatientAsync(this XmlDocument xmlDocument, string strFileName, bool blnSafe = true, int intTimeout = Utils.SleepEmergencyReleaseMaxTicks, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            bool blnSuccess = false;
            for (int i = 0; i < intTimeout; i += Utils.DefaultSleepDuration)
            {
                try
                {
                    await xmlDocument.LoadStandardFromLzmaCompressedAsync(strFileName, blnSafe, token).ConfigureAwait(false);
                    blnSuccess = true;
                    break;
                }
                catch (IOException)
                {
                    // swallow this unless we are at the emergency release stage
                    if (i >= intTimeout - Utils.DefaultSleepDuration)
                        throw;
                }

                await Utils.SafeSleepAsync(token).ConfigureAwait(false);
            }

            if (!blnSuccess)
                throw new TimeoutException();
        }

        /// <summary>
        /// Get an <see cref="XPathNavigator"/> for an <see cref="XPathDocument"/> copy of an <see cref="XmlDocument"/>.
        /// Method is slow, but the nagivator it creates is faster than that of an <see cref="XmlDocument"/>. Use accordingly.
        /// </summary>
        /// <param name="xmlDocument">The document from which a navigator should be spawned.</param>
        /// <returns>An <see cref="XPathNavigator"/> of an <see cref="XPathDocument"/> copy of <paramref name="xmlDocument"/>.</returns>
        public static XPathNavigator GetFastNavigator(this XmlDocument xmlDocument)
        {
            if (xmlDocument == null)
                return null;
            using (RecyclableMemoryStream objMemoryStream = new RecyclableMemoryStream(Utils.MemoryStreamManager))
            {
                xmlDocument.Save(objMemoryStream);
                objMemoryStream.Seek(0, SeekOrigin.Begin);
                //TODO: Should probably be using GlobalSettings.SafeXmlReaderSettings here but it has some issues.
                using (XmlReader objXmlReader = XmlReader.Create(objMemoryStream, GlobalSettings.UnSafeXmlReaderSettings))
                    return new XPathDocument(objXmlReader).CreateNavigator();
            }
        }

        /// <summary>
        /// Get an <see cref="XPathNavigator"/> for an <see cref="XPathDocument"/> copy of an <see cref="XmlDocument"/>.
        /// Method is slow, but the nagivator it creates is faster than that of an <see cref="XmlDocument"/>. Use accordingly.
        /// </summary>
        /// <param name="xmlDocument">The document from which a navigator should be spawned.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>An <see cref="XPathNavigator"/> of an <see cref="XPathDocument"/> copy of <paramref name="xmlDocument"/>.</returns>
        public static Task<XPathNavigator> GetFastNavigatorAsync(this XmlDocument xmlDocument, CancellationToken token = default)
        {
            return xmlDocument == null
                ? Task.FromResult<XPathNavigator>(null)
                : TaskExtensions.RunWithoutEC(() =>
                {
                    using (RecyclableMemoryStream objMemoryStream = new RecyclableMemoryStream(Utils.MemoryStreamManager))
                    {
                        token.ThrowIfCancellationRequested();
                        xmlDocument.Save(objMemoryStream);
                        token.ThrowIfCancellationRequested();
                        objMemoryStream.Seek(0, SeekOrigin.Begin);
                        token.ThrowIfCancellationRequested();
                        //TODO: Should probably be using GlobalSettings.SafeXmlReaderSettings here but it has some issues.
                        using (XmlReader objXmlReader
                               = XmlReader.Create(objMemoryStream, GlobalSettings.UnSafeXmlReaderSettings))
                        {
                            token.ThrowIfCancellationRequested();
                            XPathDocument objReturn = new XPathDocument(objXmlReader);
                            token.ThrowIfCancellationRequested();
                            return objReturn.CreateNavigator();
                        }
                    }
                }, token);
        }
    }
}
