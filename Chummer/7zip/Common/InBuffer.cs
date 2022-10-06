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
            int aNumProcessedBytes = m_Stream.Read(m_Buffer, 0, (int)m_BufferSize);
            m_Pos = 0;
            m_Limit = (uint)aNumProcessedBytes;
            m_StreamWasExhausted = aNumProcessedBytes == 0;
            return !m_StreamWasExhausted;
        }

        public void ReleaseStream()
        {
            // m_Stream.Close();
            m_Stream = null;
        }

        public bool ReadByte(byte b) // check it
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

        [CLSCompliant(false)]
        public ulong GetProcessedSize()
        {
            return m_ProcessedSize + m_Pos;
        }
    }
}
