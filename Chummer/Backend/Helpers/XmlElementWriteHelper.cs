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
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Chummer
{
    public readonly struct XmlElementWriteHelper : IDisposable, IAsyncDisposable, IEquatable<XmlElementWriteHelper>
    {
        private readonly XmlWriter _objWriter;

        public static XmlElementWriteHelper StartElement(XmlWriter objWriter, string localName)
        {
            objWriter.WriteStartElement(localName);
            return new XmlElementWriteHelper(objWriter);
        }

        public static async Task<XmlElementWriteHelper> StartElementAsync(XmlWriter objWriter, string localName, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await objWriter.WriteStartElementAsync(localName, token: token).ConfigureAwait(false);
            return new XmlElementWriteHelper(objWriter);
        }

        private XmlElementWriteHelper(XmlWriter objWriter)
        {
            _objWriter = objWriter;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _objWriter.WriteEndElement();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await _objWriter.WriteEndElementAsync().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public bool Equals(XmlElementWriteHelper other)
        {
            return Equals(_objWriter, other._objWriter);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is XmlElementWriteHelper other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _objWriter?.GetHashCode() ?? 0;
        }

        public static bool operator ==(XmlElementWriteHelper left, XmlElementWriteHelper right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(XmlElementWriteHelper left, XmlElementWriteHelper right)
        {
            return !(left == right);
        }
    }
}
