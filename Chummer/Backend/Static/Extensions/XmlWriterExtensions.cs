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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Chummer
{
    public static class XmlWriterExtensions
    {
        /// <inheritdoc cref="XmlWriter.WriteStartElementAsync"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task WriteStartElementAsync(this XmlWriter objWriter, string localName, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await objWriter.WriteStartElementAsync(null, localName, null).ConfigureAwait(false);
            if (objWriter.WriteState == WriteState.Error)
            {
                Utils.BreakIfDebug();
                throw new InvalidOperationException(nameof(objWriter));
            }
        }

        /// <inheritdoc cref="XmlWriter.WriteElementStringAsync"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task WriteElementStringAsync(this XmlWriter objWriter, string localName, string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await objWriter.WriteElementStringAsync(null, localName, null, value).ConfigureAwait(false);
            if (objWriter.WriteState == WriteState.Error)
            {
                Utils.BreakIfDebug();
                throw new InvalidOperationException(nameof(objWriter));
            }
        }

        /// <inheritdoc cref="XmlWriter.WriteElementStringAsync"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task WriteAttributeStringAsync(this XmlWriter objWriter, string localName, string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await objWriter.WriteAttributeStringAsync(null, localName, null, value).ConfigureAwait(false);
            if (objWriter.WriteState == WriteState.Error)
            {
                Utils.BreakIfDebug();
                throw new InvalidOperationException(nameof(objWriter));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlElementWriteHelper StartElement(this XmlWriter objWriter, string localName)
        {
            XmlElementWriteHelper objReturn = XmlElementWriteHelper.StartElement(objWriter, localName);
            if (objWriter.WriteState == WriteState.Error)
            {
                Utils.BreakIfDebug();
                throw new InvalidOperationException(nameof(objWriter));
            }
            return objReturn;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<XmlElementWriteHelper> StartElementAsync(this XmlWriter objWriter, string localName, CancellationToken token = default)
        {
            XmlElementWriteHelper objReturn = await XmlElementWriteHelper.StartElementAsync(objWriter, localName, token).ConfigureAwait(false);
            if (objWriter.WriteState == WriteState.Error)
            {
                Utils.BreakIfDebug();
                throw new InvalidOperationException(nameof(objWriter));
            }
            return objReturn;
        }
    }
}
