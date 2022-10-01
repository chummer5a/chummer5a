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

using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public static class XmlDocumentExtensions
    {
        /// <summary>
        /// Syntactic sugar for synchronously loading an XmlDocument from a file with standard encoding and XmlReader settings
        /// </summary>
        /// <param name="xmlDocument">The document into which the XML data should be loaded.</param>
        /// <param name="strFileName">The file to use.</param>
        /// <param name="blnSafe">Whether or not to check characters for validity while loading.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadStandard(this XmlDocument xmlDocument, string strFileName, bool blnSafe = true)
        {
            using (StreamReader objStreamReader = new StreamReader(strFileName, Encoding.UTF8, true))
            using (XmlReader objReader = XmlReader.Create(objStreamReader,
                blnSafe ? GlobalSettings.SafeXmlReaderSettings : GlobalSettings.UnSafeXmlReaderSettings))
                xmlDocument.Load(objReader);
        }

        /// <summary>
        /// Syntactic sugar for asynchronously loading an XmlDocument from a file with standard encoding and XmlReader settings
        /// </summary>
        /// <param name="xmlDocument">The document into which the XML data should be loaded.</param>
        /// <param name="strFileName">The file to use.</param>
        /// <param name="blnSafe">Whether or not to check characters for validity while loading.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task LoadStandardAsync(this XmlDocument xmlDocument, string strFileName, bool blnSafe = true, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                using (StreamReader objStreamReader = new StreamReader(strFileName, Encoding.UTF8, true))
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
            }, token);
        }

        /// <summary>
        /// Syntactic sugar for synchronously loading an XmlDocument from an LZMA-compressed file with standard encoding and XmlReader settings
        /// </summary>
        /// <param name="xmlDocument">The document into which the XML data should be loaded.</param>
        /// <param name="strFileName">The file to use.</param>
        /// <param name="blnSafe">Whether or not to check characters for validity while loading.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadStandardFromLzmaCompressed(this XmlDocument xmlDocument, string strFileName, bool blnSafe = true)
        {
            using (FileStream objFileStream = new FileStream(strFileName, FileMode.Open))
            using (MemoryStream objMemoryStream = new MemoryStream((int) objFileStream.Length))
            {
                objFileStream.DecompressLzmaFile(objMemoryStream);
                objMemoryStream.Seek(0, SeekOrigin.Begin);
                using (StreamReader objStreamReader
                       = new StreamReader(objMemoryStream, Encoding.UTF8, true))
                using (XmlReader objReader = XmlReader.Create(objStreamReader,
                                                              blnSafe
                                                                  ? GlobalSettings.SafeXmlReaderSettings
                                                                  : GlobalSettings.UnSafeXmlReaderSettings))
                    xmlDocument.Load(objReader);
            }
        }

        /// <summary>
        /// Syntactic sugar for asynchronously loading an XmlDocument from an LZMA-compressed file with standard encoding and XmlReader settings
        /// </summary>
        /// <param name="xmlDocument">The document into which the XML data should be loaded.</param>
        /// <param name="strFileName">The file to use.</param>
        /// <param name="blnSafe">Whether or not to check characters for validity while loading.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task LoadStandardFromLzmaCompressedAsync(this XmlDocument xmlDocument, string strFileName, bool blnSafe = true, CancellationToken token = default)
        {
            return Task.Run(async () =>
            {
                using (FileStream objFileStream = new FileStream(strFileName, FileMode.Open))
                {
                    token.ThrowIfCancellationRequested();
                    using (MemoryStream objMemoryStream = new MemoryStream((int) objFileStream.Length))
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
                                xmlDocument.Load(objReader);
                            }
                        }
                    }
                }
            }, token);
        }

        /// <summary>
        /// Get an XPathNavigator for an XPathDocument copy of an XmlDocument.
        /// Method is slow, but the nagivator it creates is faster than that of an XmlDocument. Use accordingly.
        /// </summary>
        /// <param name="xmlDocument">The document from which a navigator should be spawned.</param>
        /// <returns>An XPathNavigator of an XPathDocument copy of <paramref name="xmlDocument"/>.</returns>
        public static XPathNavigator GetFastNavigator(this XmlDocument xmlDocument)
        {
            if (xmlDocument == null)
                return null;
            using (MemoryStream objMemoryStream = new MemoryStream())
            {
                xmlDocument.Save(objMemoryStream);
                objMemoryStream.Seek(0, SeekOrigin.Begin);
                //TODO: Should probably be using GlobalSettings.SafeXmlReaderSettings here but it has some issues.
                using (XmlReader objXmlReader = XmlReader.Create(objMemoryStream, GlobalSettings.UnSafeXmlReaderSettings))
                    return new XPathDocument(objXmlReader).CreateNavigator();
            }
        }

        /// <summary>
        /// Get an XPathNavigator for an XPathDocument copy of an XmlDocument.
        /// Method is slow, but the nagivator it creates is faster than that of an XmlDocument. Use accordingly.
        /// </summary>
        /// <param name="xmlDocument">The document from which a navigator should be spawned.</param>
        /// <returns>An XPathNavigator of an XPathDocument copy of <paramref name="xmlDocument"/>.</returns>
        public static Task<XPathNavigator> GetFastNavigatorAsync(this XmlDocument xmlDocument, CancellationToken token = default)
        {
            return xmlDocument == null
                ? Task.FromResult<XPathNavigator>(null)
                : Task.Run(() =>
                {
                    using (MemoryStream objMemoryStream = new MemoryStream())
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
