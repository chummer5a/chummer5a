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
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public static class XmlDocumentExtensions
    {
        /// <summary>
        /// Syntactic sugar for loading an XmlDocument from a file with standard encoding and XmlReader settings
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
        /// Get an XPathNavigator for an XPathDocument copy of an XmlDocument.
        /// Method is slow, but the nagivator it creates is faster than that of an XmlDocument. Use accordingly.
        /// </summary>
        /// <param name="xmlDocument">The document from which a navigator should be spawned.</param>
        /// <returns>An XPathNavigator of an XPathDocument copy of <paramref name="xmlDocument"/>.</returns>
        public static XPathNavigator GetFastNavigator(this XmlDocument xmlDocument)
        {
            if (xmlDocument == null)
                return null;
            using (MemoryStream memStream = new MemoryStream())
            {
                xmlDocument.Save(memStream);
                memStream.Position = 0;
                //TODO: Should probably be using GlobalSettings.SafeXmlReaderSettings here but it has some issues.
                using (XmlReader objXmlReader = XmlReader.Create(memStream, GlobalSettings.UnSafeXmlReaderSettings))
                    return new XPathDocument(objXmlReader).CreateNavigator();
            }
        }
    }
}
