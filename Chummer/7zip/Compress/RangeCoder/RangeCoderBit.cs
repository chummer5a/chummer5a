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

namespace SevenZip.Compression.RangeCoder
{
    internal struct BitEncoder
    {
        public const int kNumBitModelTotalBits = 11;
        public const uint kBitModelTotal = 1 << kNumBitModelTotalBits;
        private const int kNumMoveBits = 5;
        private const int kNumMoveReducingBits = 2;
        public const int kNumBitPriceShiftBits = 6;

        private uint Prob;

        public void Init()
        { Prob = kBitModelTotal >> 1; }

        public void UpdateModel(uint symbol)
        {
            unchecked
            {
                if (symbol == 0)
                    Prob += (kBitModelTotal - Prob) >> kNumMoveBits;
                else
                    Prob -= Prob >> kNumMoveBits;
            }
        }

        public void Encode(Encoder encoder, uint symbol)
        {
            // encoder.EncodeBit(Prob, kNumBitModelTotalBits, symbol);
            // UpdateModel(symbol);
            unchecked
            {
                uint newBound = (encoder.Range >> kNumBitModelTotalBits) * Prob;
                if (symbol == 0)
                {
                    encoder.Range = newBound;
                    Prob += (kBitModelTotal - Prob) >> kNumMoveBits;
                }
                else
                {
                    encoder.Low += newBound;
                    encoder.Range -= newBound;
                    Prob -= Prob >> kNumMoveBits;
                }

                if (encoder.Range < Encoder.kTopValue)
                {
                    encoder.Range <<= 8;
                    encoder.ShiftLow();
                }
            }
        }

        private static readonly uint[] ProbPrices = new uint[kBitModelTotal >> kNumMoveReducingBits];

        static BitEncoder()
        {
            unchecked
            {
                const int kNumBits = kNumBitModelTotalBits - kNumMoveReducingBits;
                for (int i = kNumBits - 1; i >= 0; i--)
                {
                    uint start = (uint)1 << (kNumBits - i - 1);
                    uint end = (uint)1 << (kNumBits - i);
                    for (uint j = start; j < end; j++)
                        ProbPrices[j] = ((uint)i << kNumBitPriceShiftBits) +
                                        (((end - j) << kNumBitPriceShiftBits) >> (kNumBits - i - 1));
                }
            }
        }

        public uint GetPrice(uint symbol)
        {
            unchecked
            {
                return ProbPrices[(((Prob - symbol) ^ -(int)symbol) & (kBitModelTotal - 1)) >> kNumMoveReducingBits];
            }
        }

        public uint GetPrice0()
        { return ProbPrices[Prob >> kNumMoveReducingBits]; }

        public uint GetPrice1()
        { return ProbPrices[(kBitModelTotal - Prob) >> kNumMoveReducingBits]; }
    }

    internal struct BitDecoder
    {
        public const int kNumBitModelTotalBits = 11;
        public const uint kBitModelTotal = 1 << kNumBitModelTotalBits;
        private const int kNumMoveBits = 5;

        private uint Prob;

        public void UpdateModel(int numMoveBits, uint symbol)
        {
            unchecked
            {
                if (symbol == 0)
                    Prob += (kBitModelTotal - Prob) >> numMoveBits;
                else
                    Prob -= Prob >> numMoveBits;
            }
        }

        public void Init()
        { Prob = kBitModelTotal >> 1; }

        public uint Decode(Decoder rangeDecoder)
        {
            unchecked
            {
                uint newBound = (rangeDecoder.Range >> kNumBitModelTotalBits) * Prob;
                if (rangeDecoder.Code < newBound)
                {
                    rangeDecoder.Range = newBound;
                    Prob += (kBitModelTotal - Prob) >> kNumMoveBits;
                    if (rangeDecoder.Range < Decoder.kTopValue)
                    {
                        rangeDecoder.Code = (rangeDecoder.Code << 8) | (byte)rangeDecoder.Stream.ReadByte();
                        rangeDecoder.Range <<= 8;
                    }

                    return 0;
                }

                rangeDecoder.Range -= newBound;
                rangeDecoder.Code -= newBound;
                Prob -= Prob >> kNumMoveBits;
                if (rangeDecoder.Range < Decoder.kTopValue)
                {
                    rangeDecoder.Code = (rangeDecoder.Code << 8) | (byte)rangeDecoder.Stream.ReadByte();
                    rangeDecoder.Range <<= 8;
                }

                return 1;
            }
        }
    }
}
