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

using System.Threading;
using System.Threading.Tasks;

namespace SevenZip.Compression.RangeCoder
{
    internal readonly struct BitTreeEncoder
    {
        private readonly BitEncoder[] Models;
        private readonly int NumBitLevels;

        public BitTreeEncoder(int numBitLevels)
        {
            NumBitLevels = numBitLevels;
            Models = new BitEncoder[1 << numBitLevels];
        }

        public void Init()
        {
            unchecked
            {
                for (uint i = 1; i < 1 << NumBitLevels; i++)
                    Models[i].Init();
            }
        }

        public void Encode(Encoder rangeEncoder, uint symbol)
        {
            uint m = 1;
            unchecked
            {
                for (int bitIndex = NumBitLevels; bitIndex > 0;)
                {
                    bitIndex--;
                    uint bit = (symbol >> bitIndex) & 1;
                    Models[m].Encode(rangeEncoder, bit);
                    m = (m << 1) | bit;
                }
            }
        }

        public async Task EncodeAsync(Encoder rangeEncoder, uint symbol, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            uint m = 1;
            unchecked
            {
                for (int bitIndex = NumBitLevels; bitIndex > 0;)
                {
                    bitIndex--;
                    uint bit = (symbol >> bitIndex) & 1;
                    await Models[m].EncodeAsync(rangeEncoder, bit, token).ConfigureAwait(false);
                    m = (m << 1) | bit;
                }
            }
        }

        public void ReverseEncode(Encoder rangeEncoder, uint symbol)
        {
            uint m = 1;
            unchecked
            {
                for (uint i = 0; i < NumBitLevels; i++)
                {
                    uint bit = symbol & 1;
                    Models[m].Encode(rangeEncoder, bit);
                    m = (m << 1) | bit;
                    symbol >>= 1;
                }
            }
        }

        public async Task ReverseEncodeAsync(Encoder rangeEncoder, uint symbol, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            uint m = 1;
            unchecked
            {
                for (uint i = 0; i < NumBitLevels; i++)
                {
                    uint bit = symbol & 1;
                    await Models[m].EncodeAsync(rangeEncoder, bit, token).ConfigureAwait(false);
                    m = (m << 1) | bit;
                    symbol >>= 1;
                }
            }
        }

        public uint GetPrice(uint symbol)
        {
            uint price = 0;
            uint m = 1;
            unchecked
            {
                for (int bitIndex = NumBitLevels; bitIndex > 0;)
                {
                    bitIndex--;
                    uint bit = (symbol >> bitIndex) & 1;
                    price += Models[m].GetPrice(bit);
                    m = (m << 1) + bit;
                }
            }
            return price;
        }

        public uint ReverseGetPrice(uint symbol)
        {
            uint price = 0;
            uint m = 1;
            unchecked
            {
                for (int i = NumBitLevels; i > 0; i--)
                {
                    uint bit = symbol & 1;
                    symbol >>= 1;
                    price += Models[m].GetPrice(bit);
                    m = (m << 1) | bit;
                }
            }
            return price;
        }

        public static uint ReverseGetPrice(BitEncoder[] Models, uint startIndex,
            int NumBitLevels, uint symbol)
        {
            uint price = 0;
            uint m = 1;
            unchecked
            {
                for (int i = NumBitLevels; i > 0; i--)
                {
                    uint bit = symbol & 1;
                    symbol >>= 1;
                    price += Models[startIndex + m].GetPrice(bit);
                    m = (m << 1) | bit;
                }
            }

            return price;
        }

        public static void ReverseEncode(BitEncoder[] Models, uint startIndex,
            Encoder rangeEncoder, int NumBitLevels, uint symbol)
        {
            uint m = 1;
            unchecked
            {
                for (int i = 0; i < NumBitLevels; i++)
                {
                    uint bit = symbol & 1;
                    Models[startIndex + m].Encode(rangeEncoder, bit);
                    m = (m << 1) | bit;
                    symbol >>= 1;
                }
            }
        }

        public static async Task ReverseEncodeAsync(BitEncoder[] Models, uint startIndex,
                                         Encoder rangeEncoder, int NumBitLevels, uint symbol, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            uint m = 1;
            unchecked
            {
                for (int i = 0; i < NumBitLevels; i++)
                {
                    uint bit = symbol & 1;
                    await Models[startIndex + m].EncodeAsync(rangeEncoder, bit, token).ConfigureAwait(false);
                    m = (m << 1) | bit;
                    symbol >>= 1;
                }
            }
        }
    }

    internal readonly struct BitTreeDecoder
    {
        private readonly BitDecoder[] Models;
        private readonly int NumBitLevels;

        public BitTreeDecoder(int numBitLevels)
        {
            NumBitLevels = numBitLevels;
            Models = new BitDecoder[1 << numBitLevels];
        }

        public void Init()
        {
            unchecked
            {
                for (uint i = 1; i < 1 << NumBitLevels; i++)
                    Models[i].Init();
            }
        }

        public uint Decode(Decoder rangeDecoder)
        {
            uint m = 1;
            unchecked
            {
                for (int bitIndex = NumBitLevels; bitIndex > 0; bitIndex--)
                    m = (m << 1) + Models[m].Decode(rangeDecoder);
                return m - ((uint)1 << NumBitLevels);
            }
        }

        public async Task<uint> DecodeAsync(Decoder rangeDecoder, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            uint m = 1;
            unchecked
            {
                for (int bitIndex = NumBitLevels; bitIndex > 0; bitIndex--)
                    m = (m << 1) + await Models[m].DecodeAsync(rangeDecoder, token).ConfigureAwait(false);
                return m - ((uint)1 << NumBitLevels);
            }
        }

        public uint ReverseDecode(Decoder rangeDecoder)
        {
            uint m = 1;
            uint symbol = 0;
            unchecked
            {
                for (int bitIndex = 0; bitIndex < NumBitLevels; bitIndex++)
                {
                    uint bit = Models[m].Decode(rangeDecoder);
                    m <<= 1;
                    m += bit;
                    symbol |= bit << bitIndex;
                }
            }

            return symbol;
        }

        public static uint ReverseDecode(BitDecoder[] Models, uint startIndex,
            Decoder rangeDecoder, int NumBitLevels)
        {
            uint m = 1;
            uint symbol = 0;
            unchecked
            {
                for (int bitIndex = 0; bitIndex < NumBitLevels; bitIndex++)
                {
                    uint bit = Models[startIndex + m].Decode(rangeDecoder);
                    m <<= 1;
                    m += bit;
                    symbol |= bit << bitIndex;
                }
            }

            return symbol;
        }

        public async Task<uint> ReverseDecodeAsync(Decoder rangeDecoder, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            uint m = 1;
            uint symbol = 0;
            unchecked
            {
                for (int bitIndex = 0; bitIndex < NumBitLevels; bitIndex++)
                {
                    uint bit = await Models[m].DecodeAsync(rangeDecoder, token).ConfigureAwait(false);
                    m <<= 1;
                    m += bit;
                    symbol |= bit << bitIndex;
                }
            }

            return symbol;
        }

        public static async Task<uint> ReverseDecodeAsync(BitDecoder[] Models, uint startIndex,
                                                     Decoder rangeDecoder, int NumBitLevels, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            uint m = 1;
            uint symbol = 0;
            unchecked
            {
                for (int bitIndex = 0; bitIndex < NumBitLevels; bitIndex++)
                {
                    uint bit = await Models[startIndex + m].DecodeAsync(rangeDecoder, token).ConfigureAwait(false);
                    m <<= 1;
                    m += bit;
                    symbol |= bit << bitIndex;
                }
            }

            return symbol;
        }
    }
}
