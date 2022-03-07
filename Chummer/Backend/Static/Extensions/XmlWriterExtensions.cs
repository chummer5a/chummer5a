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

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml;

namespace Chummer
{
    public static class XmlWriterExtensions
    {
        /// <inheritdoc cref="XmlWriter.WriteStartElementAsync"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task WriteStartElementAsync(this XmlWriter objWriter, string localName)
        {
            return objWriter.WriteStartElementAsync(null, localName, null);
        }

        /// <inheritdoc cref="XmlWriter.WriteElementStringAsync"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task WriteElementStringAsync(this XmlWriter objWriter, string localName, string value)
        {
            return objWriter.WriteElementStringAsync(null, localName, null, value);
        }

        /// <inheritdoc cref="XmlWriter.WriteElementStringAsync"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task WriteAttributeStringAsync(this XmlWriter objWriter, string localName, string value)
        {
            return objWriter.WriteAttributeStringAsync(null, localName, null, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlElementWriteHelper StartElement(this XmlWriter objWriter, string localName)
        {
            return XmlElementWriteHelper.StartElement(objWriter, localName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<XmlElementWriteHelper> StartElementAsync(this XmlWriter objWriter, string localName)
        {
            return XmlElementWriteHelper.StartElementAsync(objWriter, localName);
        }
    }
}
