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
// OutBuffer.cs

using System;

namespace SevenZip.Buffer
{
    public class OutBuffer
    {
        private readonly byte[] m_Buffer;
        private uint m_Pos;
        private readonly uint m_BufferSize;
        private System.IO.Stream m_Stream;
        private ulong m_ProcessedSize;

        [CLSCompliant(false)]
        public OutBuffer(uint bufferSize)
        {
            m_Buffer = new byte[bufferSize];
            m_BufferSize = bufferSize;
        }

        public void SetStream(System.IO.Stream stream)
        { m_Stream = stream; }

        public void FlushStream()
        { m_Stream.Flush(); }

        public void CloseStream()
        { m_Stream.Close(); }

        public void ReleaseStream()
        { m_Stream = null; }

        public void Init()
        {
            m_ProcessedSize = 0;
            m_Pos = 0;
        }

        public void WriteByte(byte b)
        {
            m_Buffer[m_Pos++] = b;
            if (m_Pos >= m_BufferSize)
                FlushData();
        }

        public void FlushData()
        {
            if (m_Pos == 0)
                return;
            m_Stream.Write(m_Buffer, 0, (int)m_Pos);
            m_Pos = 0;
        }

        [CLSCompliant(false)]
        public ulong GetProcessedSize()
        { return m_ProcessedSize + m_Pos; }
    }
}
