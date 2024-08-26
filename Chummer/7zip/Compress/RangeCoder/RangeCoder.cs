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

using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SevenZip.Compression.RangeCoder
{
    internal class Encoder
    {
        public const uint kTopValue = 1 << 24;

        private Stream Stream;

        public ulong Low;
        public uint Range;
        private int _cacheSize;
        private byte _cache;

        private long StartPosition;

        public void SetStream(Stream stream)
        {
            Stream = stream;
        }

        public void ReleaseStream()
        {
            Stream = null;
        }

        public void Init()
        {
            StartPosition = Stream.Position;

            Low = 0;
            Range = 0xFFFFFFFF;
            _cacheSize = 1;
            _cache = 0;
        }

        public void FlushData()
        {
            for (int i = 0; i < 5; i++)
                ShiftLow();
        }

        public async Task FlushDataAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            for (int i = 0; i < 5; i++)
                await ShiftLowAsync(token).ConfigureAwait(false);
        }

        public void FlushStream()
        {
            Stream.Flush();
        }

        public Task FlushStreamAsync(CancellationToken token = default)
        {
            return Stream.FlushAsync(token);
        }

        public void CloseStream()
        {
            Stream.Close();
        }

        public void Encode(uint start, uint size, uint total)
        {
            unchecked
            {
                Low += start * (Range /= total);
                Range *= size;
                while (Range < kTopValue)
                {
                    Range <<= 8;
                    ShiftLow();
                }
            }
        }

        public async Task EncodeAsync(uint start, uint size, uint total, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            unchecked
            {
                Low += start * (Range /= total);
                Range *= size;
                while (Range < kTopValue)
                {
                    Range <<= 8;
                    await ShiftLowAsync(token).ConfigureAwait(false);
                }
            }
        }

        public void ShiftLow()
        {
            unchecked
            {
                uint shiftedLow = (uint) (Low >> 32);
                if ((uint)Low < 0xFF000000 || shiftedLow == 1)
                {
                    if (_cacheSize > 1)
                    {
                        byte[] data = ArrayPool<byte>.Shared.Rent(_cacheSize);
                        try
                        {
                            data[0] = (byte)(_cache + shiftedLow);
                            byte paddingValue = (byte)(0xFF + shiftedLow);
                            for (int i = 1; i < _cacheSize; ++i)
                                data[i] = paddingValue;
                            Stream.Write(data, 0, _cacheSize);
                        }
                        finally
                        {
                            ArrayPool<byte>.Shared.Return(data);
                        }
                    }
                    else
                        Stream.WriteByte((byte)(_cache + shiftedLow));
                    _cacheSize = 0;
                    _cache = (byte)((uint)Low >> 24);
                }

                _cacheSize++;
                Low = (uint)Low << 8;
            }
        }

        public async Task ShiftLowAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            unchecked
            {
                uint shiftedLow = (uint)(Low >> 32);
                if ((uint)Low < 0xFF000000 || shiftedLow == 1)
                {
                    if (_cacheSize > 1)
                    {
                        byte[] data = ArrayPool<byte>.Shared.Rent(_cacheSize);
                        try
                        {
                            data[0] = (byte)(_cache + shiftedLow);
                            byte paddingValue = (byte)(0xFF + shiftedLow);
                            for (int i = 1; i < _cacheSize; ++i)
                                data[i] = paddingValue;
                            await Stream.WriteAsync(data, 0, _cacheSize, token).ConfigureAwait(false);
                        }
                        finally
                        {
                            ArrayPool<byte>.Shared.Return(data);
                        }
                    }
                    else
                        Stream.WriteByte((byte)(_cache + shiftedLow));
                    _cacheSize = 0;
                    _cache = (byte)((uint)Low >> 24);
                }

                _cacheSize++;
                Low = (uint)Low << 8;
            }
        }

        public void EncodeDirectBits(int v, int numTotalBits)
        {
            unchecked
            {
                for (int i = numTotalBits - 1; i >= 0; i--)
                {
                    Range >>= 1;
                    if (((v >> i) & 1) == 1)
                        Low += Range;
                    if (Range < kTopValue)
                    {
                        Range <<= 8;
                        ShiftLow();
                    }
                }
            }
        }

        public async Task EncodeDirectBitsAsync(int v, int numTotalBits, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            unchecked
            {
                for (int i = numTotalBits - 1; i >= 0; i--)
                {
                    Range >>= 1;
                    if (((v >> i) & 1) == 1)
                        Low += Range;
                    if (Range < kTopValue)
                    {
                        Range <<= 8;
                        await ShiftLowAsync(token).ConfigureAwait(false);
                    }
                }
            }
        }

        public void EncodeBit(uint size0, int numTotalBits, uint symbol)
        {
            unchecked
            {
                uint newBound = (Range >> numTotalBits) * size0;
                if (symbol == 0)
                    Range = newBound;
                else
                {
                    Low += newBound;
                    Range -= newBound;
                }

                while (Range < kTopValue)
                {
                    Range <<= 8;
                    ShiftLow();
                }
            }
        }

        public async Task EncodeBitAsync(uint size0, int numTotalBits, uint symbol, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            unchecked
            {
                uint newBound = (Range >> numTotalBits) * size0;
                if (symbol == 0)
                    Range = newBound;
                else
                {
                    Low += newBound;
                    Range -= newBound;
                }

                while (Range < kTopValue)
                {
                    Range <<= 8;
                    await ShiftLowAsync(token).ConfigureAwait(false);
                }
            }
        }

        public long GetProcessedSizeAdd()
        {
            return _cacheSize +
                Stream.Position - StartPosition + 4;
            // (long)Stream.GetProcessedSize();
        }
    }

    internal class Decoder
    {
        public const uint kTopValue = 1 << 24;
        public uint Range;
        public uint Code;

        // public Buffer.InBuffer Stream = new Buffer.InBuffer(1 << 16);
        public Stream Stream;

        public void Init(Stream stream)
        {
            // Stream.Init(stream);
            Stream = stream;

            Code = 0;
            Range = 0xFFFFFFFF;
            byte[] achrBuffer = ArrayPool<byte>.Shared.Rent(5);
            try
            {
                _ = Stream.Read(achrBuffer, 0, 5);
                unchecked
                {
                    unsafe
                    {
                        fixed (byte* pchrBuffer = achrBuffer)
                        {
                            for (int i = 0; i < 5; i++)
                                Code = (Code << 8) | *(pchrBuffer + i);
                        }
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(achrBuffer);
            }
        }

        public async Task InitAsync(Stream stream, CancellationToken token = default)
        {
            // Stream.Init(stream);
            Stream = stream;

            Code = 0;
            Range = 0xFFFFFFFF;
            byte[] achrBuffer = ArrayPool<byte>.Shared.Rent(5);
            try
            {
                _ = await Stream.ReadAsync(achrBuffer, 0, 5, token).ConfigureAwait(false);
                unchecked
                {
                    unsafe
                    {
                        fixed (byte* pchrBuffer = achrBuffer)
                        {
                            for (int i = 0; i < 5; i++)
                                Code = (Code << 8) | *(pchrBuffer + i);
                        }
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(achrBuffer);
            }
        }

        public void ReleaseStream()
        {
            // Stream.ReleaseStream();
            Stream = null;
        }

        public void CloseStream()
        {
            Stream.Close();
        }

        public void Normalize()
        {
            unchecked
            {
                int intNumReads = Chummer.IntegerExtensions.DivAwayFromZero((int) (kTopValue / Range), 8);
                if (intNumReads <= 0)
                    return;
                byte[] achrBuffer = ArrayPool<byte>.Shared.Rent(intNumReads);
                try
                {
                    _ = Stream.Read(achrBuffer, 0, intNumReads);
                    int i = 0;
                    unsafe
                    {
                        fixed (byte* pchrBuffer = achrBuffer)
                        {
                            while (Range < kTopValue)
                            {
                                Code = (Code << 8) | *(pchrBuffer + i++);
                                Range <<= 8;
                            }
                        }
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(achrBuffer);
                }
            }
        }

        public async Task NormalizeAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            unchecked
            {
                int intNumReads = Chummer.IntegerExtensions.DivAwayFromZero((int)(kTopValue / Range), 8);
                if (intNumReads <= 0)
                    return;
                byte[] achrBuffer = ArrayPool<byte>.Shared.Rent(intNumReads);
                try
                {
                    _ = await Stream.ReadAsync(achrBuffer, 0, intNumReads, token).ConfigureAwait(false);
                    int i = 0;
                    unsafe
                    {
                        fixed (byte* pchrBuffer = achrBuffer)
                        {
                            while (Range < kTopValue)
                            {
                                Code = (Code << 8) | *(pchrBuffer + i++);
                                Range <<= 8;
                            }
                        }
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(achrBuffer);
                }
            }
        }

        public void Normalize2()
        {
            unchecked
            {
                if (Range < kTopValue)
                {
                    Code = (Code << 8) | (byte)Stream.ReadByte();
                    Range <<= 8;
                }
            }
        }

        public uint GetThreshold(uint total)
        {
            return Code / (Range /= total);
        }

        public void Decode(uint start, uint size, uint total)
        {
            unchecked
            {
                Code -= start * Range;
                Range *= size;
                Normalize();
            }
        }

        public Task DecodeAsync(uint start, uint size, uint total, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            unchecked
            {
                Code -= start * Range;
                Range *= size;
                return NormalizeAsync(token);
            }
        }

        public uint DecodeDirectBits(int numTotalBits)
        {
            uint range = Range;
            uint code = Code;
            uint result = 0;
            unchecked
            {
                for (int i = numTotalBits; i > 0; i--)
                {
                    range >>= 1;
                    /*
                    result <<= 1;
                    if (code >= range)
                    {
                        code -= range;
                        result |= 1;
                    }
                    */
                    uint t = (code - range) >> 31;
                    code -= range & (t - 1);
                    result = (result << 1) | (1 - t);

                    if (range < kTopValue)
                    {
                        code = (code << 8) | (byte)Stream.ReadByte();
                        range <<= 8;
                    }
                }
            }

            Range = range;
            Code = code;
            return result;
        }

        public uint DecodeBit(uint size0, int numTotalBits)
        {
            unchecked
            {
                uint newBound = (Range >> numTotalBits) * size0;
                uint symbol;
                if (Code < newBound)
                {
                    symbol = 0;
                    Range = newBound;
                }
                else
                {
                    symbol = 1;
                    Code -= newBound;
                    Range -= newBound;
                }

                Normalize();

                return symbol;
            }
        }

        public async Task<uint> DecodeBitAsync(uint size0, int numTotalBits, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            unchecked
            {
                uint newBound = (Range >> numTotalBits) * size0;
                uint symbol;
                if (Code < newBound)
                {
                    symbol = 0;
                    Range = newBound;
                }
                else
                {
                    symbol = 1;
                    Code -= newBound;
                    Range -= newBound;
                }

                await NormalizeAsync(token).ConfigureAwait(false);

                return symbol;
            }
        }

        // ulong GetProcessedSize() {return Stream.GetProcessedSize(); }
    }
}
