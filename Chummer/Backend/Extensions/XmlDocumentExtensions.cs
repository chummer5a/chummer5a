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
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public static class XmlDocumentExtensions
    {
        private static XmlReaderSettings AsyncReaderSettings { get; } = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Ignore,
            XmlResolver = null,
            CloseInput = false,
            Async = true
        };

        private static XmlReaderSettings UnsafeAsyncReaderSettings { get; } = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Ignore,
            XmlResolver = null,
            CloseInput = false,
            Async = true,
            CheckCharacters = false
        };

        /// <summary>
        /// Load an XmlDocument asynchronously from a file.
        /// </summary>
        /// <param name="xmlDocument">The document into which the XML data should be loaded.</param>
        /// <param name="strFileName">The file to use</param>
        /// <param name="blnSafe">Whether or not to check characters for validity while loading.</param>
        /// <returns></returns>
        public static async Task LoadAsync(this XmlDocument xmlDocument, string strFileName, bool blnSafe = true)
        {
            if (xmlDocument == null)
                throw new ArgumentNullException(nameof(xmlDocument));
            if (string.IsNullOrEmpty(strFileName))
                throw new ArgumentNullException(nameof(strFileName));
            using (StreamReader objStreamReader = new StreamReader(strFileName, Encoding.UTF8, true))
                using (XmlReader objReader = XmlReader.Create(objStreamReader, blnSafe ? AsyncReaderSettings : UnsafeAsyncReaderSettings))
                    await xmlDocument.LoadAsync(objReader).ConfigureAwait(false);
        }

        /// <summary>
        /// Load an XmlDocument asynchronously from a stream.
        /// </summary>
        /// <param name="xmlDocument">The document into which the XML data should be loaded.</param>
        /// <param name="objStream">The stream from which to load.</param>
        /// <param name="blnSafe">Whether or not to check characters for validity while loading.</param>
        /// <returns></returns>
        public static async Task LoadAsync(this XmlDocument xmlDocument, Stream objStream, bool blnSafe = true)
        {
            if (xmlDocument == null)
                throw new ArgumentNullException(nameof(xmlDocument));
            if (objStream == null)
                throw new ArgumentNullException(nameof(objStream));
            using (XmlReader objReader = XmlReader.Create(objStream, blnSafe ? AsyncReaderSettings : UnsafeAsyncReaderSettings))
                await xmlDocument.LoadAsync(objReader).ConfigureAwait(false);
        }

        /// <summary>
        /// Load an XmlDocument asynchronously from an XmlReader.
        /// Adapted from https://stackoverflow.com/a/65827589, which was made for XDocument instead of XmlDocument.
        /// </summary>
        /// <param name="xmlDocument">The document into which the XML data should be loaded.</param>
        /// <param name="objReader">The XmlReader from which to load.</param>
        /// <returns></returns>
        public static async Task LoadAsync(this XmlDocument xmlDocument, XmlReader objReader)
        {
            if (xmlDocument == null)
                throw new ArgumentNullException(nameof(xmlDocument));
            if (objReader == null)
                throw new ArgumentNullException(nameof(objReader));
            XPathNavigator xmlDocumentNavigator = xmlDocument.CreateNavigator();
            if (xmlDocumentNavigator == null)
                return;
            using (XmlWriter objWriter = xmlDocumentNavigator.AppendChild())
            {
                do
                {
                    switch (objReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            await objWriter.WriteStartElementAsync(objReader.Prefix, objReader.LocalName, objReader.NamespaceURI).ConfigureAwait(false);
                            await objWriter.WriteAttributesAsync(objReader, true).ConfigureAwait(false);
                            if (objReader.IsEmptyElement)
                                await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                            break;
                        case XmlNodeType.Text:
                            {
                                string strValue = await objReader.GetValueAsync().ConfigureAwait(false);
                                await objWriter.WriteStringAsync(strValue).ConfigureAwait(false);
                                break;
                            }
                        case XmlNodeType.CDATA:
                            await objWriter.WriteCDataAsync(objReader.Value).ConfigureAwait(false);
                            break;
                        case XmlNodeType.EntityReference:
                            await objWriter.WriteEntityRefAsync(objReader.Name).ConfigureAwait(false);
                            break;
                        case XmlNodeType.ProcessingInstruction:
                        case XmlNodeType.XmlDeclaration:
                            await objWriter.WriteProcessingInstructionAsync(objReader.Name, objReader.Value).ConfigureAwait(false);
                            break;
                        case XmlNodeType.Comment:
                            await objWriter.WriteCommentAsync(objReader.Value).ConfigureAwait(false);
                            break;
                        case XmlNodeType.DocumentType:
                            await objWriter.WriteDocTypeAsync(objReader.Name, objReader.GetAttribute("PUBLIC"), objReader.GetAttribute("SYSTEM"), objReader.Value).ConfigureAwait(false);
                            break;
                        case XmlNodeType.Whitespace:
                        case XmlNodeType.SignificantWhitespace:
                            {
                                string strValue = await objReader.GetValueAsync().ConfigureAwait(false);
                                await objWriter.WriteWhitespaceAsync(strValue).ConfigureAwait(false);
                                break;
                            }
                        case XmlNodeType.EndElement:
                            await objWriter.WriteFullEndElementAsync().ConfigureAwait(false);
                            break;
                    }
                } while (await objReader.ReadAsync().ConfigureAwait(false));
            }
        }
    }
}
