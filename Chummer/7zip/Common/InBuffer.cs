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
// InBuffer.cs

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SevenZip.Buffer
{
    public class InBuffer
    {
        private readonly byte[] m_Buffer;
        private uint m_Pos;
        private uint m_Limit;
        private readonly uint m_BufferSize;
        private System.IO.Stream m_Stream;
        private bool m_StreamWasExhausted;
        private ulong m_ProcessedSize;

        [CLSCompliant(false)]
        public InBuffer(uint bufferSize)
        {
            m_Buffer = new byte[bufferSize];
            m_BufferSize = bufferSize;
        }

        public void Init(System.IO.Stream stream)
        {
            m_Stream = stream;
            m_ProcessedSize = 0;
            m_Limit = 0;
            m_Pos = 0;
            m_StreamWasExhausted = false;
        }

        public bool ReadBlock()
        {
            if (m_StreamWasExhausted)
                return false;
            m_ProcessedSize += m_Pos;
            uint aNumProcessedBytes;
            if (m_BufferSize > int.MaxValue)
            {
                int intToRead1 = (int)(m_BufferSize / 2);
                int intToRead2 = intToRead1 + (int)(m_BufferSize & 1);
                aNumProcessedBytes = (uint)m_Stream.Read(m_Buffer, 0, intToRead1) + (uint)m_Stream.Read(m_Buffer, intToRead1, intToRead2);
            }
            else
                aNumProcessedBytes = (uint)m_Stream.Read(m_Buffer, 0, (int)m_BufferSize);
            m_Pos = 0;
            m_Limit = aNumProcessedBytes;
            m_StreamWasExhausted = aNumProcessedBytes == 0;
            return !m_StreamWasExhausted;
        }

        public async Task<bool> ReadBlockAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (m_StreamWasExhausted)
                return false;
            m_ProcessedSize += m_Pos;
            uint aNumProcessedBytes;
            if (m_BufferSize > int.MaxValue)
            {
                int intToRead1 = (int)(m_BufferSize / 2);
                int intToRead2 = intToRead1 + (int)(m_BufferSize & 1);
                Task<int> tskRead1 = m_Stream.ReadAsync(m_Buffer, 0, intToRead1, token);
                Task<int> tskRead2 = m_Stream.ReadAsync(m_Buffer, intToRead1, intToRead2, token);
                await Task.WhenAll(tskRead1, tskRead2).ConfigureAwait(false);
                aNumProcessedBytes = (uint)await tskRead1.ConfigureAwait(false) + (uint)await tskRead2.ConfigureAwait(false);
            }
            else
                aNumProcessedBytes = (uint)await m_Stream.ReadAsync(m_Buffer, 0, (int)m_BufferSize, token).ConfigureAwait(false);
            m_Pos = 0;
            m_Limit = aNumProcessedBytes;
            m_StreamWasExhausted = aNumProcessedBytes == 0;
            return !m_StreamWasExhausted;
        }

        public void ReleaseStream()
        {
            // m_Stream.Close();
            m_Stream = null;
        }

        public bool ReadByte(ref byte b) // check it
        {
            if (m_Pos >= m_Limit && !ReadBlock())
                return false;
            b = m_Buffer[m_Pos++];
            return true;
        }

        public byte ReadByte()
        {
            // return (byte)m_Stream.ReadByte();
            if (m_Pos >= m_Limit && !ReadBlock())
                return 0xFF;
            return m_Buffer[m_Pos++];
        }

        public async Task<Tuple<bool, byte>> ReadByteAsync(CancellationToken token = default)
        {
            if (m_Pos >= m_Limit && !await ReadBlockAsync(token).ConfigureAwait(false))
                return new Tuple<bool, byte>(false, 0xFF);
            return new Tuple<bool, byte>(true, m_Buffer[m_Pos++]);
        }

        [CLSCompliant(false)]
        public ulong GetProcessedSize()
        {
            return m_ProcessedSize + m_Pos;
        }
    }
}
