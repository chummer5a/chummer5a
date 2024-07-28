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
// LzInWindow.cs

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SevenZip.Compression.LZ
{
    [CLSCompliant(false)]
    public class InWindow
    {
        public byte[] _bufferBase; // pointer to buffer with data
        private Stream _stream;
        private int _posLimit; // offset (from _buffer) of first byte when new block reading must be done
        protected bool _streamEndWasReached; // if (true) then _streamPos shows real end of stream

        private int _pointerToLastSafePosition;
        
        public int _bufferOffset;
        
        public int _blockSize; // Size of Allocated memory block
        public int _pos; // offset (from _buffer) of curent byte
        private int _keepSizeBefore; // how many BYTEs must be kept in buffer before _pos
        private int _keepSizeAfter; // how many BYTEs must be kept buffer after _pos
        public int _streamPos; // offset (from _buffer) of first not read byte from Stream

        public unsafe void MoveBlock()
        {
            unchecked
            {
                int offset = _bufferOffset + _pos - _keepSizeBefore;
                // we need one additional byte, since MovePos moves on 1 byte.
                if (offset > 0)
                    offset--;

                int numBytes = _bufferOffset + _streamPos - offset;

                // check negative offset ????
                fixed (byte* pchrBufferBase = &_bufferBase[offset])
                {
                    for (int i = 0; i < numBytes; i++)
                        _bufferBase[i] = *(pchrBufferBase + i);
                }

                _bufferOffset -= offset;
            }
        }

        public virtual void ReadBlock()
        {
            if (_streamEndWasReached)
                return;
            unchecked
            {
                while (true)
                {
                    int size = 0 - _bufferOffset + _blockSize - _streamPos;
                    if (size == 0)
                        return;
                    int numReadBytes = _stream.Read(_bufferBase, _bufferOffset + _streamPos, size);
                    if (numReadBytes == 0)
                    {
                        _posLimit = _streamPos;
                        int pointerToPosition = _bufferOffset + _posLimit;
                        if (pointerToPosition > _pointerToLastSafePosition)
                            _posLimit = _pointerToLastSafePosition - _bufferOffset;

                        _streamEndWasReached = true;
                        return;
                    }

                    _streamPos += numReadBytes;
                    if (_streamPos >= _pos + _keepSizeAfter)
                        _posLimit = _streamPos - _keepSizeAfter;
                }
            }
        }

        public virtual async Task ReadBlockAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_streamEndWasReached)
                return;
            unchecked
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    int size = 0 - _bufferOffset + _blockSize - _streamPos;
                    if (size == 0)
                        return;
                    int numReadBytes = await _stream.ReadAsync(_bufferBase, _bufferOffset + _streamPos, size, token).ConfigureAwait(false);
                    if (numReadBytes == 0)
                    {
                        _posLimit = _streamPos;
                        int pointerToPosition = _bufferOffset + _posLimit;
                        if (pointerToPosition > _pointerToLastSafePosition)
                            _posLimit = _pointerToLastSafePosition - _bufferOffset;

                        _streamEndWasReached = true;
                        return;
                    }

                    _streamPos += numReadBytes;
                    if (_streamPos >= _pos + _keepSizeAfter)
                        _posLimit = _streamPos - _keepSizeAfter;
                }
            }
        }

        private void Free()
        { _bufferBase = null; }

        [CLSCompliant(false)]
        public void Create(int keepSizeBefore, int keepSizeAfter, int keepSizeReserve)
        {
            if (keepSizeBefore > 2147483591) // System.Array.MaxByteArrayLength
                throw new ArgumentOutOfRangeException(nameof(keepSizeBefore));
            _keepSizeBefore = keepSizeBefore;
            if (keepSizeBefore + keepSizeAfter > 2147483591) // System.Array.MaxByteArrayLength
                throw new ArgumentOutOfRangeException(nameof(keepSizeAfter));
            _keepSizeAfter = keepSizeAfter;
            int blockSize = keepSizeBefore + keepSizeAfter + keepSizeReserve;
            if (blockSize > 2147483591) // System.Array.MaxByteArrayLength
                throw new ArgumentOutOfRangeException(nameof(blockSize));
            if (_bufferBase == null || _blockSize != blockSize)
            {
                Free();
                _blockSize = blockSize;
                _bufferBase = new byte[_blockSize];
            }
            _pointerToLastSafePosition = _blockSize - keepSizeAfter;
        }

        public virtual void SetStream(Stream stream)
        { _stream = stream; }

        public virtual void ReleaseStream()
        { _stream = null; }

        public virtual void Init()
        {
            _bufferOffset = 0;
            _pos = 0;
            _streamPos = 0;
            _streamEndWasReached = false;
            ReadBlock();
        }

        public virtual Task InitAsync(CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            _bufferOffset = 0;
            _pos = 0;
            _streamPos = 0;
            _streamEndWasReached = false;
            return ReadBlockAsync(token);
        }

        public virtual void MovePos()
        {
            unchecked
            {
                _pos++;
                if (_pos > _posLimit)
                {
                    int pointerToPosition = _bufferOffset + _pos;
                    if (pointerToPosition > _pointerToLastSafePosition)
                        MoveBlock();
                    ReadBlock();
                }
            }
        }

        public virtual Task MovePosAsync(CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);

            unchecked
            {
                _pos++;
                if (_pos > _posLimit)
                {
                    int pointerToPosition = _bufferOffset + _pos;
                    if (pointerToPosition > _pointerToLastSafePosition)
                        MoveBlock();
                    return ReadBlockAsync(token);
                }
            }

            return Task.CompletedTask;
        }

        public virtual byte GetIndexByte(int index)
        {
            unchecked
            {
                return _bufferBase[_bufferOffset + _pos + index];
            }
        }

        // index + limit have not to exceed _keepSizeAfter;
        [CLSCompliant(false)]
        public virtual unsafe int GetMatchLen(int index, int distance, int limit)
        {
            unchecked
            {
                if (_streamEndWasReached && _pos + index + limit > _streamPos)
                    limit = _streamPos - (_pos + index);
                distance++;
                // Byte *pby = _buffer + (size_t)_pos + index;
                int pby = _bufferOffset + _pos + index;
                int pby2 = pby - distance;
                int i = 0;
                fixed (byte* p1 = &_bufferBase[pby])
                fixed (byte* p2 = &_bufferBase[pby2])
                {
                    // First do equality comparisons 8 bytes at a time to speed things up
                    const int size = sizeof(ulong);
                    if (limit >= size)
                    {
                        int longLimit = limit - size + 1;
                        while (i < longLimit && *(ulong*)(p1 + i) == *(ulong*)(p2 + i))
                        {
                            i += size;
                        }
                    }
                    while (i < limit && *(p1 + i) == *(p2 + i))
                        ++i;
                }

                return i;
            }
        }

        public virtual int GetNumAvailableBytes()
        { return _streamPos - _pos; }

        public void ReduceOffsets(int subValue)
        {
            _bufferOffset += subValue;
            _posLimit -= subValue;
            _pos -= subValue;
            _streamPos -= subValue;
        }
    }
}
