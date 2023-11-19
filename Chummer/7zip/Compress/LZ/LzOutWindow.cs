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
// LzOutWindow.cs

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SevenZip.Compression.LZ
{
    [CLSCompliant(false)]
    public class OutWindow
    {
        private byte[] _buffer;
        private uint _pos;
        private uint _windowSize;
        private uint _streamPos;
        private Stream _stream;

        public uint TrainSize;

        [CLSCompliant(false)]
        public void Create(uint windowSize)
        {
            if (_windowSize != windowSize)
            {
                // System.GC.Collect();
                _buffer = new byte[windowSize];
            }
            _windowSize = windowSize;
            _pos = 0;
            _streamPos = 0;
        }

        public void Init(Stream stream, bool solid)
        {
            ReleaseStream();
            _stream = stream;
            if (!solid)
            {
                _streamPos = 0;
                _pos = 0;
                TrainSize = 0;
            }
        }

        public async Task InitAsync(Stream stream, bool solid, CancellationToken token = default)
        {
            await ReleaseStreamAsync(token).ConfigureAwait(false);
            _stream = stream;
            if (!solid)
            {
                _streamPos = 0;
                _pos = 0;
                TrainSize = 0;
            }
        }

        public bool Train(Stream stream)
        {
            unchecked
            {
                long len = stream.Length;
                uint size = len < _windowSize ? (uint) len : _windowSize;
                TrainSize = size;
                stream.Position = len - size;
                _streamPos = _pos = 0;
                while (size > 0)
                {
                    uint curSize = _windowSize - _pos;
                    if (size < curSize)
                        curSize = size;
                    int numReadBytes = stream.Read(_buffer, (int) _pos, (int) curSize);
                    if (numReadBytes == 0)
                        return false;
                    size -= (uint) numReadBytes;
                    _pos += (uint) numReadBytes;
                    _streamPos += (uint) numReadBytes;
                    if (_pos == _windowSize)
                        _streamPos = _pos = 0;
                }
            }
            return true;
        }

        public async Task<bool> TrainAsync(Stream stream, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            unchecked
            {
                long len = stream.Length;
                uint size = len < _windowSize ? (uint)len : _windowSize;
                TrainSize = size;
                stream.Position = len - size;
                _streamPos = _pos = 0;
                while (size > 0)
                {
                    token.ThrowIfCancellationRequested();
                    uint curSize = _windowSize - _pos;
                    if (size < curSize)
                        curSize = size;
                    int numReadBytes = await stream.ReadAsync(_buffer, (int)_pos, (int)curSize, token).ConfigureAwait(false);
                    if (numReadBytes == 0)
                        return false;
                    size -= (uint)numReadBytes;
                    _pos += (uint)numReadBytes;
                    _streamPos += (uint)numReadBytes;
                    if (_pos == _windowSize)
                        _streamPos = _pos = 0;
                }
            }
            return true;
        }

        public void ReleaseStream()
        {
            Flush();
            _stream = null;
        }

        public async Task ReleaseStreamAsync(CancellationToken token = default)
        {
            await FlushAsync(token).ConfigureAwait(false);
            _stream = null;
        }

        public void Flush()
        {
            uint size = _pos - _streamPos;
            if (size == 0)
                return;
            _stream.Write(_buffer, (int)_streamPos, (int)size);
            if (_pos >= _windowSize)
                _pos = 0;
            _streamPos = _pos;
        }

        public async Task FlushAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            uint size = _pos - _streamPos;
            if (size == 0)
                return;
            await _stream.WriteAsync(_buffer, (int)_streamPos, (int)size, token).ConfigureAwait(false);
            if (_pos >= _windowSize)
                _pos = 0;
            _streamPos = _pos;
        }

        [CLSCompliant(false)]
        public void CopyBlock(uint distance, uint len)
        {
            unchecked
            {
                uint pos = _pos - distance - 1;
                if (pos >= _windowSize)
                    pos += _windowSize;
                for (; len > 0; len--)
                {
                    if (pos >= _windowSize)
                        pos = 0;
                    _buffer[_pos++] = _buffer[pos++];
                    if (_pos >= _windowSize)
                        Flush();
                }
            }
        }

        [CLSCompliant(false)]
        public async Task CopyBlockAsync(uint distance, uint len, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            unchecked
            {
                uint pos = _pos - distance - 1;
                if (pos >= _windowSize)
                    pos += _windowSize;
                for (; len > 0; len--)
                {
                    if (pos >= _windowSize)
                        pos = 0;
                    _buffer[_pos++] = _buffer[pos++];
                    if (_pos >= _windowSize)
                        await FlushAsync(token).ConfigureAwait(false);
                }
            }
        }

        public void PutByte(byte b)
        {
            _buffer[_pos++] = b;
            if (_pos >= _windowSize)
                Flush();
        }

        public Task PutByteAsync(byte b, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            _buffer[_pos++] = b;
            return _pos >= _windowSize ? FlushAsync(token) : Task.CompletedTask;
        }

        [CLSCompliant(false)]
        public byte GetByte(uint distance)
        {
            unchecked
            {
                uint pos = _pos - distance - 1;
                if (pos >= _windowSize)
                    pos += _windowSize;
                return _buffer[pos];
            }
        }
    }
}
