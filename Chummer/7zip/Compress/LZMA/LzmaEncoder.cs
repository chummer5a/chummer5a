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
// LzmaEncoder.cs

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SevenZip.Compression.LZ;
using SevenZip.Compression.RangeCoder;

namespace SevenZip.Compression.LZMA
{
    public class Encoder : ICoder, ISetCoderProperties, IWriteCoderProperties
    {
        public enum EMatchFinderType
        {
            BT2 = 0,
            BT4,
        }

        private const uint kInfinityPrice = 0xFFFFFFF;

        private static readonly byte[] g_FastPos = new byte[1 << 11];

        static Encoder()
        {
            const byte kFastSlots = 22;
            int c = 2;
            g_FastPos[0] = 0;
            g_FastPos[1] = 1;
            unchecked
            {
                for (byte slotFast = 2; slotFast < kFastSlots; slotFast++)
                {
                    uint k = (uint)1 << ((slotFast >> 1) - 1);
                    for (uint j = 0; j < k; j++, c++)
                        g_FastPos[c] = slotFast;
                }
            }
        }

        private static uint GetPosSlot(uint pos)
        {
            unchecked
            {
                if (pos < 1 << 11)
                    return g_FastPos[pos];
                if (pos < 1 << 21)
                    return (uint)(g_FastPos[pos >> 10] + 20);
                return (uint)(g_FastPos[pos >> 20] + 40);
            }
        }

        private static uint GetPosSlot2(uint pos)
        {
            unchecked
            {
                if (pos < 1 << 17)
                    return (uint)(g_FastPos[pos >> 6] + 12);
                if (pos < 1 << 27)
                    return (uint)(g_FastPos[pos >> 16] + 32);
                return (uint)(g_FastPos[pos >> 26] + 52);
            }
        }

        private Base.State _state;
        private byte _previousByte;
        private readonly uint[] _repDistances = new uint[Base.kNumRepDistances];

        private void BaseInit()
        {
            _state.Init();
            _previousByte = 0;
            for (uint i = 0; i < Base.kNumRepDistances; i++)
                _repDistances[i] = 0;
        }

        public const int kDefaultDictionaryLogSize = 22;
        [CLSCompliant(false)]
        public const uint kNumFastBytesDefault = 0x20;

        private sealed class LiteralEncoder
        {
            public struct Encoder2
            {
                private BitEncoder[] m_Encoders;

                public void Create()
                { m_Encoders = new BitEncoder[0x300]; }

                public void Init()
                { for (int i = 0; i < 0x300; i++) m_Encoders[i].Init(); }

                public void Encode(RangeCoder.Encoder rangeEncoder, byte symbol)
                {
                    uint context = 1;
                    unchecked
                    {
                        for (int i = 7; i >= 0; i--)
                        {
                            uint bit = (uint)((symbol >> i) & 1);
                            m_Encoders[context].Encode(rangeEncoder, bit);
                            context = (context << 1) | bit;
                        }
                    }
                }

                public async Task EncodeAsync(RangeCoder.Encoder rangeEncoder, byte symbol, CancellationToken token = default)
                {
                    token.ThrowIfCancellationRequested();
                    uint context = 1;
                    unchecked
                    {
                        for (int i = 7; i >= 0; i--)
                        {
                            uint bit = (uint)((symbol >> i) & 1);
                            await m_Encoders[context].EncodeAsync(rangeEncoder, bit, token).ConfigureAwait(false);
                            context = (context << 1) | bit;
                        }
                    }
                }

                public void EncodeMatched(RangeCoder.Encoder rangeEncoder, byte matchByte, byte symbol)
                {
                    uint context = 1;
                    bool same = true;
                    unchecked
                    {
                        for (int i = 7; i >= 0; i--)
                        {
                            uint bit = (uint)((symbol >> i) & 1);
                            uint state = context;
                            if (same)
                            {
                                uint matchBit = (uint)((matchByte >> i) & 1);
                                state += (1 + matchBit) << 8;
                                same = matchBit == bit;
                            }

                            m_Encoders[state].Encode(rangeEncoder, bit);
                            context = (context << 1) | bit;
                        }
                    }
                }

                public async Task EncodeMatchedAsync(RangeCoder.Encoder rangeEncoder, byte matchByte, byte symbol, CancellationToken token = default)
                {
                    token.ThrowIfCancellationRequested();
                    uint context = 1;
                    bool same = true;
                    unchecked
                    {
                        for (int i = 7; i >= 0; i--)
                        {
                            uint bit = (uint)((symbol >> i) & 1);
                            uint state = context;
                            if (same)
                            {
                                uint matchBit = (uint)((matchByte >> i) & 1);
                                state += (1 + matchBit) << 8;
                                same = matchBit == bit;
                            }

                            await m_Encoders[state].EncodeAsync(rangeEncoder, bit, token).ConfigureAwait(false);
                            context = (context << 1) | bit;
                        }
                    }
                }

                public uint GetPrice(bool matchMode, byte matchByte, byte symbol)
                {
                    uint price = 0;
                    uint context = 1;
                    int i = 7;
                    unchecked
                    {
                        if (matchMode)
                        {
                            for (; i >= 0; i--)
                            {
                                uint matchBit = (uint)(matchByte >> i) & 1;
                                uint bit = (uint)(symbol >> i) & 1;
                                price += m_Encoders[((1 + matchBit) << 8) + context].GetPrice(bit);
                                context = (context << 1) | bit;
                                if (matchBit != bit)
                                {
                                    i--;
                                    break;
                                }
                            }
                        }

                        for (; i >= 0; i--)
                        {
                            uint bit = (uint)(symbol >> i) & 1;
                            price += m_Encoders[context].GetPrice(bit);
                            context = (context << 1) | bit;
                        }
                    }

                    return price;
                }
            }

            private Encoder2[] m_Coders;
            private int m_NumPrevBits;
            private int m_NumPosBits;
            private uint m_PosMask;

            public void Create(int numPosBits, int numPrevBits)
            {
                if (m_Coders != null && m_NumPrevBits == numPrevBits && m_NumPosBits == numPosBits)
                    return;
                m_NumPosBits = numPosBits;
                unchecked
                {
                    m_PosMask = ((uint)1 << numPosBits) - 1;
                    m_NumPrevBits = numPrevBits;
                    uint numStates = (uint)1 << (m_NumPrevBits + m_NumPosBits);
                    m_Coders = new Encoder2[numStates];
                    for (uint i = 0; i < numStates; i++)
                        m_Coders[i].Create();
                }
            }

            public void Init()
            {
                unchecked
                {
                    uint numStates = (uint)1 << (m_NumPrevBits + m_NumPosBits);
                    for (uint i = 0; i < numStates; i++)
                        m_Coders[i].Init();
                }
            }

            public Encoder2 GetSubCoder(uint pos, byte prevByte)
            {
                unchecked
                {
                    return m_Coders[((pos & m_PosMask) << m_NumPrevBits) + (uint)(prevByte >> (8 - m_NumPrevBits))];
                }
            }
        }

        private class LenEncoder
        {
            private BitEncoder _choice;
            private BitEncoder _choice2;
            private readonly BitTreeEncoder[] _lowCoder = new BitTreeEncoder[Base.kNumPosStatesEncodingMax];
            private readonly BitTreeEncoder[] _midCoder = new BitTreeEncoder[Base.kNumPosStatesEncodingMax];
            private readonly BitTreeEncoder _highCoder = new BitTreeEncoder(Base.kNumHighLenBits);

            public LenEncoder()
            {
                for (uint posState = 0; posState < Base.kNumPosStatesEncodingMax; posState++)
                {
                    _lowCoder[posState] = new BitTreeEncoder(Base.kNumLowLenBits);
                    _midCoder[posState] = new BitTreeEncoder(Base.kNumMidLenBits);
                }
            }

            public void Init(uint numPosStates)
            {
                _choice.Init();
                _choice2.Init();
                for (uint posState = 0; posState < numPosStates; posState++)
                {
                    _lowCoder[posState].Init();
                    _midCoder[posState].Init();
                }
                _highCoder.Init();
            }

            public void Encode(RangeCoder.Encoder rangeEncoder, uint symbol, uint posState)
            {
                if (symbol < Base.kNumLowLenSymbols)
                {
                    _choice.Encode(rangeEncoder, 0);
                    _lowCoder[posState].Encode(rangeEncoder, symbol);
                }
                else
                {
                    symbol -= Base.kNumLowLenSymbols;
                    _choice.Encode(rangeEncoder, 1);
                    if (symbol < Base.kNumMidLenSymbols)
                    {
                        _choice2.Encode(rangeEncoder, 0);
                        _midCoder[posState].Encode(rangeEncoder, symbol);
                    }
                    else
                    {
                        _choice2.Encode(rangeEncoder, 1);
                        _highCoder.Encode(rangeEncoder, symbol - Base.kNumMidLenSymbols);
                    }
                }
            }

            public async Task EncodeAsync(RangeCoder.Encoder rangeEncoder, uint symbol, uint posState, CancellationToken token = default)
            {
                token.ThrowIfCancellationRequested();
                if (symbol < Base.kNumLowLenSymbols)
                {
                    await _choice.EncodeAsync(rangeEncoder, 0, token).ConfigureAwait(false);
                    await _lowCoder[posState].EncodeAsync(rangeEncoder, symbol, token).ConfigureAwait(false);
                }
                else
                {
                    symbol -= Base.kNumLowLenSymbols;
                    await _choice.EncodeAsync(rangeEncoder, 1, token).ConfigureAwait(false);
                    if (symbol < Base.kNumMidLenSymbols)
                    {
                        await _choice2.EncodeAsync(rangeEncoder, 0, token).ConfigureAwait(false);
                        await _midCoder[posState].EncodeAsync(rangeEncoder, symbol, token).ConfigureAwait(false);
                    }
                    else
                    {
                        await _choice2.EncodeAsync(rangeEncoder, 1, token).ConfigureAwait(false);
                        await _highCoder.EncodeAsync(rangeEncoder, symbol - Base.kNumMidLenSymbols, token).ConfigureAwait(false);
                    }
                }
            }

            public void SetPrices(uint posState, uint numSymbols, uint[] prices, uint st)
            {
                uint a0 = _choice.GetPrice0();
                uint a1 = _choice.GetPrice1();
                uint b0 = a1 + _choice2.GetPrice0();
                uint b1 = a1 + _choice2.GetPrice1();
                uint i;
                for (i = 0; i < Base.kNumLowLenSymbols; i++)
                {
                    if (i >= numSymbols)
                        return;
                    prices[st + i] = a0 + _lowCoder[posState].GetPrice(i);
                }
                for (; i < kNumLenSpecSymbols; i++)
                {
                    if (i >= numSymbols)
                        return;
                    prices[st + i] = b0 + _midCoder[posState].GetPrice(i - Base.kNumLowLenSymbols);
                }
                for (; i < numSymbols; i++)
                    prices[st + i] = b1 + _highCoder.GetPrice(i - kNumLenSpecSymbols);
            }
        }

        private const uint kNumLenSpecSymbols = Base.kNumLowLenSymbols + Base.kNumMidLenSymbols;

        private class LenPriceTableEncoder : LenEncoder
        {
            private readonly uint[] _prices = new uint[Base.kNumLenSymbols << Base.kNumPosStatesBitsEncodingMax];
            private uint _tableSize;
            private readonly uint[] _counters = new uint[Base.kNumPosStatesEncodingMax];

            public void SetTableSize(uint tableSize)
            { _tableSize = tableSize; }

            public uint GetPrice(uint symbol, uint posState)
            {
                return _prices[posState * Base.kNumLenSymbols + symbol];
            }

            private void UpdateTable(uint posState)
            {
                SetPrices(posState, _tableSize, _prices, posState * Base.kNumLenSymbols);
                _counters[posState] = _tableSize;
            }

            public void UpdateTables(uint numPosStates)
            {
                for (uint posState = 0; posState < numPosStates; posState++)
                    UpdateTable(posState);
            }

            public new void Encode(RangeCoder.Encoder rangeEncoder, uint symbol, uint posState)
            {
                base.Encode(rangeEncoder, symbol, posState);
                if (--_counters[posState] == 0)
                    UpdateTable(posState);
            }

            public new async Task EncodeAsync(RangeCoder.Encoder rangeEncoder, uint symbol, uint posState, CancellationToken token = default)
            {
                await base.EncodeAsync(rangeEncoder, symbol, posState, token).ConfigureAwait(false);
                if (--_counters[posState] == 0)
                    UpdateTable(posState);
            }
        }

        private const uint kNumOpts = 1 << 12;

        private sealed class Optimal
        {
            public Base.State State;

            public bool Prev1IsChar;
            public bool Prev2;

            public uint PosPrev2;
            public uint BackPrev2;

            public uint Price;
            public uint PosPrev;
            public uint BackPrev;

            public uint Backs0;
            public uint Backs1;
            public uint Backs2;
            public uint Backs3;

            public void MakeAsChar()
            { BackPrev = 0xFFFFFFFF; Prev1IsChar = false; }

            public void MakeAsShortRep()
            { BackPrev = 0; Prev1IsChar = false; }

            public bool IsShortRep()
            { return BackPrev == 0; }
        }

        private readonly Optimal[] _optimum = new Optimal[kNumOpts];
        private IMatchFinder _matchFinder;
        private readonly RangeCoder.Encoder _rangeEncoder = new RangeCoder.Encoder();

        private readonly BitEncoder[] _isMatch = new BitEncoder[Base.kNumStates << Base.kNumPosStatesBitsMax];
        private readonly BitEncoder[] _isRep = new BitEncoder[Base.kNumStates];
        private readonly BitEncoder[] _isRepG0 = new BitEncoder[Base.kNumStates];
        private readonly BitEncoder[] _isRepG1 = new BitEncoder[Base.kNumStates];
        private readonly BitEncoder[] _isRepG2 = new BitEncoder[Base.kNumStates];
        private readonly BitEncoder[] _isRep0Long = new BitEncoder[Base.kNumStates << Base.kNumPosStatesBitsMax];

        private readonly BitTreeEncoder[] _posSlotEncoder = new BitTreeEncoder[Base.kNumLenToPosStates];

        private readonly BitEncoder[] _posEncoders = new BitEncoder[Base.kNumFullDistances - Base.kEndPosModelIndex];
        private readonly BitTreeEncoder _posAlignEncoder = new BitTreeEncoder(Base.kNumAlignBits);

        private readonly LenPriceTableEncoder _lenEncoder = new LenPriceTableEncoder();
        private readonly LenPriceTableEncoder _repMatchLenEncoder = new LenPriceTableEncoder();

        private readonly LiteralEncoder _literalEncoder = new LiteralEncoder();

        private readonly uint[] _matchDistances = new uint[Base.kMatchMaxLen * 2 + 2];

        private uint _numFastBytes = kNumFastBytesDefault;
        private uint _longestMatchLength;
        private uint _numDistancePairs;

        private uint _additionalOffset;

        private uint _optimumEndIndex;
        private uint _optimumCurrentIndex;

        private bool _longestMatchWasFound;

        private readonly uint[] _posSlotPrices = new uint[1 << (Base.kNumPosSlotBits + Base.kNumLenToPosStatesBits)];
        private readonly uint[] _distancesPrices = new uint[Base.kNumFullDistances << Base.kNumLenToPosStatesBits];
        private readonly uint[] _alignPrices = new uint[Base.kAlignTableSize];
        private uint _alignPriceCount;

        private uint _distTableSize = kDefaultDictionaryLogSize * 2;

        private int _posStateBits = 2;
        private uint _posStateMask = 4 - 1;
        private int _numLiteralPosStateBits;
        private int _numLiteralContextBits = 3;

        private uint _dictionarySize = 1 << kDefaultDictionaryLogSize;
        private uint _dictionarySizePrev = 0xFFFFFFFF;
        private uint _numFastBytesPrev = 0xFFFFFFFF;

        private long _nowPos64;
        private bool _finished;
        private Stream _inStream;

        private EMatchFinderType _matchFinderType = EMatchFinderType.BT4;
        private bool _writeEndMark;

        private bool _needReleaseMFStream;

        private void Create()
        {
            if (_matchFinder == null)
            {
                BinTree bt = new BinTree();
                int numHashBytes = 4;
                if (_matchFinderType == EMatchFinderType.BT2)
                    numHashBytes = 2;
                bt.SetType(numHashBytes);
                _matchFinder = bt;
            }
            _literalEncoder.Create(_numLiteralPosStateBits, _numLiteralContextBits);

            if (_dictionarySize == _dictionarySizePrev && _numFastBytesPrev == _numFastBytes)
                return;
            _matchFinder.Create(_dictionarySize, kNumOpts, _numFastBytes, Base.kMatchMaxLen + 1);
            _dictionarySizePrev = _dictionarySize;
            _numFastBytesPrev = _numFastBytes;
        }

        public Encoder()
        {
            for (int i = 0; i < kNumOpts; i++)
                _optimum[i] = new Optimal();
            for (int i = 0; i < Base.kNumLenToPosStates; i++)
                _posSlotEncoder[i] = new BitTreeEncoder(Base.kNumPosSlotBits);
        }

        private void SetWriteEndMarkerMode(bool writeEndMarker)
        {
            _writeEndMark = writeEndMarker;
        }

        private void Init(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            BaseInit();
            token.ThrowIfCancellationRequested();
            _rangeEncoder.Init();

            uint i;
            for (i = 0; i < Base.kNumStates; i++)
            {
                token.ThrowIfCancellationRequested();
                for (uint j = 0; j <= _posStateMask; j++)
                {
                    token.ThrowIfCancellationRequested();
                    uint complexState = (i << Base.kNumPosStatesBitsMax) + j;
                    _isMatch[complexState].Init();
                    _isRep0Long[complexState].Init();
                }
                _isRep[i].Init();
                _isRepG0[i].Init();
                _isRepG1[i].Init();
                _isRepG2[i].Init();
            }
            token.ThrowIfCancellationRequested();
            _literalEncoder.Init();
            for (i = 0; i < Base.kNumLenToPosStates; i++)
            {
                token.ThrowIfCancellationRequested();
                _posSlotEncoder[i].Init();
            }

            for (i = 0; i < Base.kNumFullDistances - Base.kEndPosModelIndex; i++)
            {
                token.ThrowIfCancellationRequested();
                _posEncoders[i].Init();
            }

            token.ThrowIfCancellationRequested();
            _lenEncoder.Init((uint)1 << _posStateBits);
            _repMatchLenEncoder.Init((uint)1 << _posStateBits);

            token.ThrowIfCancellationRequested();
            _posAlignEncoder.Init();

            _longestMatchWasFound = false;
            _optimumEndIndex = 0;
            _optimumCurrentIndex = 0;
            _additionalOffset = 0;
        }

        private void ReadMatchDistances(out uint lenRes, out uint numDistancePairs)
        {
            lenRes = 0;
            numDistancePairs = _matchFinder.GetMatches(_matchDistances);
            if (numDistancePairs > 0)
            {
                lenRes = _matchDistances[numDistancePairs - 2];
                if (lenRes == _numFastBytes)
                    lenRes += _matchFinder.GetMatchLen((int)lenRes - 1, _matchDistances[numDistancePairs - 1],
                        Base.kMatchMaxLen - lenRes);
            }
            _additionalOffset++;
        }

        private async Task<Tuple<uint, uint>> ReadMatchDistancesAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            uint lenRes = 0;
            uint numDistancePairs = await _matchFinder.GetMatchesAsync(_matchDistances, token).ConfigureAwait(false);
            if (numDistancePairs > 0)
            {
                lenRes = _matchDistances[numDistancePairs - 2];
                if (lenRes == _numFastBytes)
                    lenRes += _matchFinder.GetMatchLen((int)lenRes - 1, _matchDistances[numDistancePairs - 1],
                                                       Base.kMatchMaxLen - lenRes);
            }
            _additionalOffset++;
            return new Tuple<uint, uint>(lenRes, numDistancePairs);
        }

        private void MovePos(uint num)
        {
            if (num > 0)
            {
                _matchFinder.Skip(num);
                _additionalOffset += num;
            }
        }

        private async Task MovePosAsync(uint num, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (num > 0)
            {
                await _matchFinder.SkipAsync(num, token).ConfigureAwait(false);
                _additionalOffset += num;
            }
        }

        private uint GetRepLen1Price(Base.State state, uint posState)
        {
            unchecked
            {
                return _isRepG0[state.Index].GetPrice0() +
                       _isRep0Long[(state.Index << Base.kNumPosStatesBitsMax) + posState].GetPrice0();
            }
        }

        private uint GetPureRepPrice(uint repIndex, Base.State state, uint posState)
        {
            unchecked
            {
                uint price;
                if (repIndex == 0)
                {
                    price = _isRepG0[state.Index].GetPrice0();
                    price += _isRep0Long[(state.Index << Base.kNumPosStatesBitsMax) + posState].GetPrice1();
                }
                else
                {
                    price = _isRepG0[state.Index].GetPrice1();
                    if (repIndex == 1)
                        price += _isRepG1[state.Index].GetPrice0();
                    else
                    {
                        price += _isRepG1[state.Index].GetPrice1();
                        price += _isRepG2[state.Index].GetPrice(repIndex - 2);
                    }
                }

                return price;
            }
        }

        private uint GetRepPrice(uint repIndex, uint len, Base.State state, uint posState)
        {
            uint price = _repMatchLenEncoder.GetPrice(len - Base.kMatchMinLen, posState);
            return price + GetPureRepPrice(repIndex, state, posState);
        }

        private uint GetPosLenPrice(uint pos, uint len, uint posState)
        {
            uint price;
            uint lenToPosState = Base.GetLenToPosState(len);
            unchecked
            {
                if (pos < Base.kNumFullDistances)
                    price = _distancesPrices[lenToPosState * Base.kNumFullDistances + pos];
                else
                    price = _posSlotPrices[(lenToPosState << Base.kNumPosSlotBits) + GetPosSlot2(pos)] +
                            _alignPrices[pos & Base.kAlignMask];
            }

            return price + _lenEncoder.GetPrice(len - Base.kMatchMinLen, posState);
        }

        private uint Backward(out uint backRes, uint cur)
        {
            _optimumEndIndex = cur;
            uint posMem = _optimum[cur].PosPrev;
            uint backMem = _optimum[cur].BackPrev;
            do
            {
                if (_optimum[cur].Prev1IsChar)
                {
                    _optimum[posMem].MakeAsChar();
                    _optimum[posMem].PosPrev = posMem - 1;
                    if (_optimum[cur].Prev2)
                    {
                        _optimum[posMem - 1].Prev1IsChar = false;
                        _optimum[posMem - 1].PosPrev = _optimum[cur].PosPrev2;
                        _optimum[posMem - 1].BackPrev = _optimum[cur].BackPrev2;
                    }
                }
                uint posPrev = posMem;
                uint backCur = backMem;

                backMem = _optimum[posPrev].BackPrev;
                posMem = _optimum[posPrev].PosPrev;

                _optimum[posPrev].BackPrev = backCur;
                _optimum[posPrev].PosPrev = cur;
                cur = posPrev;
            }
            while (cur > 0);
            backRes = _optimum[0].BackPrev;
            _optimumCurrentIndex = _optimum[0].PosPrev;
            return _optimumCurrentIndex;
        }

        private readonly uint[] reps = new uint[Base.kNumRepDistances];
        private readonly uint[] repLens = new uint[Base.kNumRepDistances];

        private uint GetOptimum(uint position, out uint backRes)
        {
            if (_optimumEndIndex != _optimumCurrentIndex)
            {
                uint lenRes = _optimum[_optimumCurrentIndex].PosPrev - _optimumCurrentIndex;
                backRes = _optimum[_optimumCurrentIndex].BackPrev;
                _optimumCurrentIndex = _optimum[_optimumCurrentIndex].PosPrev;
                return lenRes;
            }
            _optimumCurrentIndex = _optimumEndIndex = 0;

            uint lenMain, numDistancePairs;
            if (!_longestMatchWasFound)
            {
                ReadMatchDistances(out lenMain, out numDistancePairs);
            }
            else
            {
                lenMain = _longestMatchLength;
                numDistancePairs = _numDistancePairs;
                _longestMatchWasFound = false;
            }

            uint numAvailableBytes = _matchFinder.GetNumAvailableBytes() + 1;
            if (numAvailableBytes < 2)
            {
                backRes = 0xFFFFFFFF;
                return 1;
            }
            if (numAvailableBytes > Base.kMatchMaxLen)
                numAvailableBytes = Base.kMatchMaxLen;

            uint repMaxIndex = 0;
            uint i;
            for (i = 0; i < Base.kNumRepDistances; i++)
            {
                reps[i] = _repDistances[i];
                repLens[i] = _matchFinder.GetMatchLen(0 - 1, reps[i], Base.kMatchMaxLen);
                if (repLens[i] > repLens[repMaxIndex])
                    repMaxIndex = i;
            }
            if (repLens[repMaxIndex] >= _numFastBytes)
            {
                backRes = repMaxIndex;
                uint lenRes = repLens[repMaxIndex];
                MovePos(lenRes - 1);
                return lenRes;
            }

            if (lenMain >= _numFastBytes)
            {
                backRes = _matchDistances[numDistancePairs - 1] + Base.kNumRepDistances;
                MovePos(lenMain - 1);
                return lenMain;
            }

            byte currentByte = _matchFinder.GetIndexByte(0 - 1);
            int index;
            unchecked
            {
                index = (int)(0 - _repDistances[0] - 1 - 1);
            }
            byte matchByte = _matchFinder.GetIndexByte(index);

            if (lenMain < 2 && currentByte != matchByte && repLens[repMaxIndex] < 2)
            {
                backRes = 0xFFFFFFFF;
                return 1;
            }

            _optimum[0].State = _state;

            uint posState = position & _posStateMask;

            unchecked
            {
                _optimum[1].Price = _isMatch[(_state.Index << Base.kNumPosStatesBitsMax) + posState].GetPrice0() +
                                    _literalEncoder.GetSubCoder(position, _previousByte)
                                                   .GetPrice(!_state.IsCharState(), matchByte, currentByte);
                _optimum[1].MakeAsChar();

                uint matchPrice = _isMatch[(_state.Index << Base.kNumPosStatesBitsMax) + posState].GetPrice1();
                uint repMatchPrice = matchPrice + _isRep[_state.Index].GetPrice1();

                if (matchByte == currentByte)
                {
                    uint shortRepPrice = repMatchPrice + GetRepLen1Price(_state, posState);
                    if (shortRepPrice < _optimum[1].Price)
                    {
                        _optimum[1].Price = shortRepPrice;
                        _optimum[1].MakeAsShortRep();
                    }
                }

                uint lenEnd = lenMain >= repLens[repMaxIndex] ? lenMain : repLens[repMaxIndex];

                if (lenEnd < 2)
                {
                    backRes = _optimum[1].BackPrev;
                    return 1;
                }

                _optimum[1].PosPrev = 0;

                _optimum[0].Backs0 = reps[0];
                _optimum[0].Backs1 = reps[1];
                _optimum[0].Backs2 = reps[2];
                _optimum[0].Backs3 = reps[3];

                uint len = lenEnd;
                do
                    _optimum[len--].Price = kInfinityPrice;
                while (len >= 2);

                for (i = 0; i < Base.kNumRepDistances; i++)
                {
                    uint repLen = repLens[i];
                    if (repLen < 2)
                        continue;
                    uint price = repMatchPrice + GetPureRepPrice(i, _state, posState);
                    do
                    {
                        uint curAndLenPrice = price + _repMatchLenEncoder.GetPrice(repLen - 2, posState);
                        Optimal optimum = _optimum[repLen];
                        if (curAndLenPrice < optimum.Price)
                        {
                            optimum.Price = curAndLenPrice;
                            optimum.PosPrev = 0;
                            optimum.BackPrev = i;
                            optimum.Prev1IsChar = false;
                        }
                    } while (--repLen >= 2);
                }

                uint normalMatchPrice = matchPrice + _isRep[_state.Index].GetPrice0();

                len = repLens[0] >= 2 ? repLens[0] + 1 : 2;
                if (len <= lenMain)
                {
                    uint offs = 0;
                    while (len > _matchDistances[offs])
                        offs += 2;
                    for (; ; len++)
                    {
                        uint distance = _matchDistances[offs + 1];
                        uint curAndLenPrice = normalMatchPrice + GetPosLenPrice(distance, len, posState);
                        Optimal optimum = _optimum[len];
                        if (curAndLenPrice < optimum.Price)
                        {
                            optimum.Price = curAndLenPrice;
                            optimum.PosPrev = 0;
                            optimum.BackPrev = distance + Base.kNumRepDistances;
                            optimum.Prev1IsChar = false;
                        }

                        if (len == _matchDistances[offs])
                        {
                            offs += 2;
                            if (offs == numDistancePairs)
                                break;
                        }
                    }
                }

                uint cur = 0;

                while (true)
                {
                    cur++;
                    if (cur == lenEnd)
                        return Backward(out backRes, cur);
                    ReadMatchDistances(out uint newLen, out numDistancePairs);
                    if (newLen >= _numFastBytes)
                    {
                        _numDistancePairs = numDistancePairs;
                        _longestMatchLength = newLen;
                        _longestMatchWasFound = true;
                        return Backward(out backRes, cur);
                    }

                    position++;
                    uint posPrev = _optimum[cur].PosPrev;
                    Base.State state;
                    if (_optimum[cur].Prev1IsChar)
                    {
                        posPrev--;
                        if (_optimum[cur].Prev2)
                        {
                            state = _optimum[_optimum[cur].PosPrev2].State;
                            if (_optimum[cur].BackPrev2 < Base.kNumRepDistances)
                                state.UpdateRep();
                            else
                                state.UpdateMatch();
                        }
                        else
                            state = _optimum[posPrev].State;

                        state.UpdateChar();
                    }
                    else
                        state = _optimum[posPrev].State;

                    if (posPrev == cur - 1)
                    {
                        if (_optimum[cur].IsShortRep())
                            state.UpdateShortRep();
                        else
                            state.UpdateChar();
                    }
                    else
                    {
                        uint pos;
                        if (_optimum[cur].Prev1IsChar && _optimum[cur].Prev2)
                        {
                            posPrev = _optimum[cur].PosPrev2;
                            pos = _optimum[cur].BackPrev2;
                            state.UpdateRep();
                        }
                        else
                        {
                            pos = _optimum[cur].BackPrev;
                            if (pos < Base.kNumRepDistances)
                                state.UpdateRep();
                            else
                                state.UpdateMatch();
                        }

                        Optimal opt = _optimum[posPrev];
                        if (pos < Base.kNumRepDistances)
                        {
                            switch (pos)
                            {
                                case 0:
                                    reps[0] = opt.Backs0;
                                    reps[1] = opt.Backs1;
                                    reps[2] = opt.Backs2;
                                    reps[3] = opt.Backs3;
                                    break;
                                case 1:
                                    reps[0] = opt.Backs1;
                                    reps[1] = opt.Backs0;
                                    reps[2] = opt.Backs2;
                                    reps[3] = opt.Backs3;
                                    break;
                                case 2:
                                    reps[0] = opt.Backs2;
                                    reps[1] = opt.Backs0;
                                    reps[2] = opt.Backs1;
                                    reps[3] = opt.Backs3;
                                    break;
                                default:
                                    reps[0] = opt.Backs3;
                                    reps[1] = opt.Backs0;
                                    reps[2] = opt.Backs1;
                                    reps[3] = opt.Backs2;
                                    break;
                            }
                        }
                        else
                        {
                            reps[0] = pos - Base.kNumRepDistances;
                            reps[1] = opt.Backs0;
                            reps[2] = opt.Backs1;
                            reps[3] = opt.Backs2;
                        }
                    }

                    _optimum[cur].State = state;
                    _optimum[cur].Backs0 = reps[0];
                    _optimum[cur].Backs1 = reps[1];
                    _optimum[cur].Backs2 = reps[2];
                    _optimum[cur].Backs3 = reps[3];
                    uint curPrice = _optimum[cur].Price;

                    currentByte = _matchFinder.GetIndexByte(0 - 1);
                    matchByte = _matchFinder.GetIndexByte((int)(0 - reps[0] - 1 - 1));

                    posState = position & _posStateMask;

                    uint curAnd1Price = curPrice +
                                        _isMatch[(state.Index << Base.kNumPosStatesBitsMax) + posState].GetPrice0() +
                                        _literalEncoder.GetSubCoder(position, _matchFinder.GetIndexByte(0 - 2))
                                                       .GetPrice(!state.IsCharState(), matchByte, currentByte);

                    Optimal nextOptimum = _optimum[cur + 1];

                    bool nextIsChar = false;
                    if (curAnd1Price < nextOptimum.Price)
                    {
                        nextOptimum.Price = curAnd1Price;
                        nextOptimum.PosPrev = cur;
                        nextOptimum.MakeAsChar();
                        nextIsChar = true;
                    }

                    matchPrice = curPrice + _isMatch[(state.Index << Base.kNumPosStatesBitsMax) + posState].GetPrice1();
                    repMatchPrice = matchPrice + _isRep[state.Index].GetPrice1();

                    if (matchByte == currentByte &&
                        !(nextOptimum.PosPrev < cur && nextOptimum.BackPrev == 0))
                    {
                        uint shortRepPrice = repMatchPrice + GetRepLen1Price(state, posState);
                        if (shortRepPrice <= nextOptimum.Price)
                        {
                            nextOptimum.Price = shortRepPrice;
                            nextOptimum.PosPrev = cur;
                            nextOptimum.MakeAsShortRep();
                            nextIsChar = true;
                        }
                    }

                    uint numAvailableBytesFull = _matchFinder.GetNumAvailableBytes() + 1;
                    numAvailableBytesFull = Math.Min(kNumOpts - 1 - cur, numAvailableBytesFull);
                    numAvailableBytes = numAvailableBytesFull;

                    if (numAvailableBytes < 2)
                        continue;
                    if (numAvailableBytes > _numFastBytes)
                        numAvailableBytes = _numFastBytes;
                    if (!nextIsChar && matchByte != currentByte)
                    {
                        // try Literal + rep0
                        uint t = Math.Min(numAvailableBytesFull - 1, _numFastBytes);
                        uint lenTest2 = _matchFinder.GetMatchLen(0, reps[0], t);
                        if (lenTest2 >= 2)
                        {
                            Base.State state2 = state;
                            state2.UpdateChar();
                            uint posStateNext = (position + 1) & _posStateMask;
                            uint nextRepMatchPrice = curAnd1Price +
                                                     _isMatch[
                                                             (state2.Index << Base.kNumPosStatesBitsMax) + posStateNext]
                                                         .GetPrice1() +
                                                     _isRep[state2.Index].GetPrice1();
                            uint offset = cur + 1 + lenTest2;
                            while (lenEnd < offset)
                                _optimum[++lenEnd].Price = kInfinityPrice;
                            uint curAndLenPrice = nextRepMatchPrice + GetRepPrice(
                                0, lenTest2, state2, posStateNext);
                            Optimal optimum = _optimum[offset];
                            if (curAndLenPrice < optimum.Price)
                            {
                                optimum.Price = curAndLenPrice;
                                optimum.PosPrev = cur + 1;
                                optimum.BackPrev = 0;
                                optimum.Prev1IsChar = true;
                                optimum.Prev2 = false;
                            }
                        }
                    }

                    uint startLen = 2; // speed optimization

                    for (uint repIndex = 0; repIndex < Base.kNumRepDistances; repIndex++)
                    {
                        uint lenTest = _matchFinder.GetMatchLen(0 - 1, reps[repIndex], numAvailableBytes);
                        if (lenTest < 2)
                            continue;
                        uint lenTestTemp = lenTest;
                        do
                        {
                            while (lenEnd < cur + lenTest)
                                _optimum[++lenEnd].Price = kInfinityPrice;
                            uint curAndLenPrice = repMatchPrice + GetRepPrice(repIndex, lenTest, state, posState);
                            Optimal optimum = _optimum[cur + lenTest];
                            if (curAndLenPrice < optimum.Price)
                            {
                                optimum.Price = curAndLenPrice;
                                optimum.PosPrev = cur;
                                optimum.BackPrev = repIndex;
                                optimum.Prev1IsChar = false;
                            }
                        } while (--lenTest >= 2);

                        lenTest = lenTestTemp;

                        if (repIndex == 0)
                            startLen = lenTest + 1;

                        // if (_maxMode)
                        if (lenTest < numAvailableBytesFull)
                        {
                            uint t = Math.Min(numAvailableBytesFull - 1 - lenTest, _numFastBytes);
                            uint lenTest2 = _matchFinder.GetMatchLen((int)lenTest, reps[repIndex], t);
                            if (lenTest2 >= 2)
                            {
                                Base.State state2 = state;
                                state2.UpdateRep();
                                uint posStateNext = (position + lenTest) & _posStateMask;
                                uint curAndLenCharPrice =
                                    repMatchPrice + GetRepPrice(repIndex, lenTest, state, posState) +
                                    _isMatch[(state2.Index << Base.kNumPosStatesBitsMax) + posStateNext].GetPrice0() +
                                    _literalEncoder.GetSubCoder(position + lenTest,
                                                                _matchFinder.GetIndexByte((int)lenTest - 2))
                                                   .GetPrice(true,
                                                             _matchFinder.GetIndexByte((int)lenTest - 1
                                                                 - (int)(reps[repIndex] + 1)),
                                                             _matchFinder.GetIndexByte((int)lenTest - 1));
                                state2.UpdateChar();
                                posStateNext = (position + lenTest + 1) & _posStateMask;
                                uint nextMatchPrice = curAndLenCharPrice
                                                      + _isMatch[
                                                              (state2.Index << Base.kNumPosStatesBitsMax)
                                                              + posStateNext]
                                                          .GetPrice1();
                                uint nextRepMatchPrice = nextMatchPrice + _isRep[state2.Index].GetPrice1();

                                // for(; lenTest2 >= 2; lenTest2--)
                                {
                                    uint offset = lenTest + 1 + lenTest2;
                                    while (lenEnd < cur + offset)
                                        _optimum[++lenEnd].Price = kInfinityPrice;
                                    uint curAndLenPrice
                                        = nextRepMatchPrice + GetRepPrice(0, lenTest2, state2, posStateNext);
                                    Optimal optimum = _optimum[cur + offset];
                                    if (curAndLenPrice < optimum.Price)
                                    {
                                        optimum.Price = curAndLenPrice;
                                        optimum.PosPrev = cur + lenTest + 1;
                                        optimum.BackPrev = 0;
                                        optimum.Prev1IsChar = true;
                                        optimum.Prev2 = true;
                                        optimum.PosPrev2 = cur;
                                        optimum.BackPrev2 = repIndex;
                                    }
                                }
                            }
                        }
                    }

                    if (newLen > numAvailableBytes)
                    {
                        newLen = numAvailableBytes;
                        for (numDistancePairs = 0; newLen > _matchDistances[numDistancePairs]; numDistancePairs += 2)
                        {
                        }

                        _matchDistances[numDistancePairs] = newLen;
                        numDistancePairs += 2;
                    }

                    if (newLen >= startLen)
                    {
                        normalMatchPrice = matchPrice + _isRep[state.Index].GetPrice0();
                        while (lenEnd < cur + newLen)
                            _optimum[++lenEnd].Price = kInfinityPrice;

                        uint offs = 0;
                        while (startLen > _matchDistances[offs])
                            offs += 2;

                        for (uint lenTest = startLen; ; lenTest++)
                        {
                            uint curBack = _matchDistances[offs + 1];
                            uint curAndLenPrice = normalMatchPrice + GetPosLenPrice(curBack, lenTest, posState);
                            Optimal optimum = _optimum[cur + lenTest];
                            if (curAndLenPrice < optimum.Price)
                            {
                                optimum.Price = curAndLenPrice;
                                optimum.PosPrev = cur;
                                optimum.BackPrev = curBack + Base.kNumRepDistances;
                                optimum.Prev1IsChar = false;
                            }

                            if (lenTest == _matchDistances[offs])
                            {
                                if (lenTest < numAvailableBytesFull)
                                {
                                    uint t = Math.Min(numAvailableBytesFull - 1 - lenTest, _numFastBytes);
                                    uint lenTest2 = _matchFinder.GetMatchLen((int)lenTest, curBack, t);
                                    if (lenTest2 >= 2)
                                    {
                                        Base.State state2 = state;
                                        state2.UpdateMatch();
                                        uint posStateNext = (position + lenTest) & _posStateMask;
                                        uint curAndLenCharPrice = curAndLenPrice +
                                                                  _isMatch[
                                                                      (state2.Index << Base.kNumPosStatesBitsMax)
                                                                      + posStateNext].GetPrice0() +
                                                                  _literalEncoder.GetSubCoder(position + lenTest,
                                                                          _matchFinder.GetIndexByte((int)lenTest - 2))
                                                                      .GetPrice(true,
                                                                          _matchFinder.GetIndexByte(
                                                                              (int)lenTest - (int)(curBack + 1)
                                                                              - 1),
                                                                          _matchFinder.GetIndexByte(
                                                                              (int)lenTest - 1));
                                        state2.UpdateChar();
                                        posStateNext = (position + lenTest + 1) & _posStateMask;
                                        uint nextMatchPrice = curAndLenCharPrice
                                                              + _isMatch[
                                                                  (state2.Index << Base.kNumPosStatesBitsMax)
                                                                  + posStateNext].GetPrice1();
                                        uint nextRepMatchPrice = nextMatchPrice + _isRep[state2.Index].GetPrice1();

                                        uint offset = lenTest + 1 + lenTest2;
                                        while (lenEnd < cur + offset)
                                            _optimum[++lenEnd].Price = kInfinityPrice;
                                        curAndLenPrice = nextRepMatchPrice
                                                         + GetRepPrice(0, lenTest2, state2, posStateNext);
                                        optimum = _optimum[cur + offset];
                                        if (curAndLenPrice < optimum.Price)
                                        {
                                            optimum.Price = curAndLenPrice;
                                            optimum.PosPrev = cur + lenTest + 1;
                                            optimum.BackPrev = 0;
                                            optimum.Prev1IsChar = true;
                                            optimum.Prev2 = true;
                                            optimum.PosPrev2 = cur;
                                            optimum.BackPrev2 = curBack + Base.kNumRepDistances;
                                        }
                                    }
                                }

                                offs += 2;
                                if (offs == numDistancePairs)
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private async Task<Tuple<uint, uint>> GetOptimumAsync(uint position, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_optimumEndIndex != _optimumCurrentIndex)
            {
                uint lenRes = _optimum[_optimumCurrentIndex].PosPrev - _optimumCurrentIndex;
                uint backRes = _optimum[_optimumCurrentIndex].BackPrev;
                _optimumCurrentIndex = _optimum[_optimumCurrentIndex].PosPrev;
                return new Tuple<uint, uint>(lenRes, backRes);
            }
            _optimumCurrentIndex = _optimumEndIndex = 0;

            uint lenMain, numDistancePairs;
            if (!_longestMatchWasFound)
            {
                (lenMain, numDistancePairs) = await ReadMatchDistancesAsync(token).ConfigureAwait(false);
            }
            else
            {
                lenMain = _longestMatchLength;
                numDistancePairs = _numDistancePairs;
                _longestMatchWasFound = false;
            }

            uint numAvailableBytes = _matchFinder.GetNumAvailableBytes() + 1;
            if (numAvailableBytes < 2)
            {
                return new Tuple<uint, uint>(1, 0xFFFFFFFF);
            }
            if (numAvailableBytes > Base.kMatchMaxLen)
                numAvailableBytes = Base.kMatchMaxLen;

            uint repMaxIndex = 0;
            uint i;
            for (i = 0; i < Base.kNumRepDistances; i++)
            {
                reps[i] = _repDistances[i];
                repLens[i] = _matchFinder.GetMatchLen(0 - 1, reps[i], Base.kMatchMaxLen);
                if (repLens[i] > repLens[repMaxIndex])
                    repMaxIndex = i;
            }
            if (repLens[repMaxIndex] >= _numFastBytes)
            {
                uint lenRes = repLens[repMaxIndex];
                await MovePosAsync(lenRes - 1, token).ConfigureAwait(false);
                return new Tuple<uint, uint>(lenRes, repMaxIndex);
            }

            if (lenMain >= _numFastBytes)
            {
                uint backRes = _matchDistances[numDistancePairs - 1] + Base.kNumRepDistances;
                await MovePosAsync(lenMain - 1, token).ConfigureAwait(false);
                return new Tuple<uint, uint>(lenMain, backRes);
            }

            byte currentByte = _matchFinder.GetIndexByte(0 - 1);
            int index;
            unchecked
            {
                index = (int)(0 - _repDistances[0] - 1 - 1);
            }
            byte matchByte = _matchFinder.GetIndexByte(index);

            if (lenMain < 2 && currentByte != matchByte && repLens[repMaxIndex] < 2)
            {
                return new Tuple<uint, uint>(1, 0xFFFFFFFF);
            }

            _optimum[0].State = _state;

            uint posState = position & _posStateMask;

            unchecked
            {
                _optimum[1].Price = _isMatch[(_state.Index << Base.kNumPosStatesBitsMax) + posState].GetPrice0() +
                                    _literalEncoder.GetSubCoder(position, _previousByte)
                                                   .GetPrice(!_state.IsCharState(), matchByte, currentByte);
                _optimum[1].MakeAsChar();

                uint matchPrice = _isMatch[(_state.Index << Base.kNumPosStatesBitsMax) + posState].GetPrice1();
                uint repMatchPrice = matchPrice + _isRep[_state.Index].GetPrice1();

                if (matchByte == currentByte)
                {
                    uint shortRepPrice = repMatchPrice + GetRepLen1Price(_state, posState);
                    if (shortRepPrice < _optimum[1].Price)
                    {
                        _optimum[1].Price = shortRepPrice;
                        _optimum[1].MakeAsShortRep();
                    }
                }

                uint lenEnd = lenMain >= repLens[repMaxIndex] ? lenMain : repLens[repMaxIndex];

                if (lenEnd < 2)
                {
                    return new Tuple<uint, uint>(1, _optimum[1].BackPrev);
                }

                _optimum[1].PosPrev = 0;

                _optimum[0].Backs0 = reps[0];
                _optimum[0].Backs1 = reps[1];
                _optimum[0].Backs2 = reps[2];
                _optimum[0].Backs3 = reps[3];

                uint len = lenEnd;
                do
                    _optimum[len--].Price = kInfinityPrice;
                while (len >= 2);

                for (i = 0; i < Base.kNumRepDistances; i++)
                {
                    uint repLen = repLens[i];
                    if (repLen < 2)
                        continue;
                    uint price = repMatchPrice + GetPureRepPrice(i, _state, posState);
                    do
                    {
                        uint curAndLenPrice = price + _repMatchLenEncoder.GetPrice(repLen - 2, posState);
                        Optimal optimum = _optimum[repLen];
                        if (curAndLenPrice < optimum.Price)
                        {
                            optimum.Price = curAndLenPrice;
                            optimum.PosPrev = 0;
                            optimum.BackPrev = i;
                            optimum.Prev1IsChar = false;
                        }
                    } while (--repLen >= 2);
                }

                uint normalMatchPrice = matchPrice + _isRep[_state.Index].GetPrice0();

                len = repLens[0] >= 2 ? repLens[0] + 1 : 2;
                if (len <= lenMain)
                {
                    uint offs = 0;
                    while (len > _matchDistances[offs])
                        offs += 2;
                    for (; ; len++)
                    {
                        uint distance = _matchDistances[offs + 1];
                        uint curAndLenPrice = normalMatchPrice + GetPosLenPrice(distance, len, posState);
                        Optimal optimum = _optimum[len];
                        if (curAndLenPrice < optimum.Price)
                        {
                            optimum.Price = curAndLenPrice;
                            optimum.PosPrev = 0;
                            optimum.BackPrev = distance + Base.kNumRepDistances;
                            optimum.Prev1IsChar = false;
                        }

                        if (len == _matchDistances[offs])
                        {
                            offs += 2;
                            if (offs == numDistancePairs)
                                break;
                        }
                    }
                }

                uint cur = 0;

                while (true)
                {
                    cur++;
                    if (cur == lenEnd)
                        return new Tuple<uint, uint>(Backward(out uint backRes, cur), backRes);
                    uint newLen;
                    (newLen, numDistancePairs) = await ReadMatchDistancesAsync(token).ConfigureAwait(false);
                    if (newLen >= _numFastBytes)
                    {
                        _numDistancePairs = numDistancePairs;
                        _longestMatchLength = newLen;
                        _longestMatchWasFound = true;
                        return new Tuple<uint, uint>(Backward(out uint backRes, cur), backRes);
                    }

                    position++;
                    uint posPrev = _optimum[cur].PosPrev;
                    Base.State state;
                    if (_optimum[cur].Prev1IsChar)
                    {
                        posPrev--;
                        if (_optimum[cur].Prev2)
                        {
                            state = _optimum[_optimum[cur].PosPrev2].State;
                            if (_optimum[cur].BackPrev2 < Base.kNumRepDistances)
                                state.UpdateRep();
                            else
                                state.UpdateMatch();
                        }
                        else
                            state = _optimum[posPrev].State;

                        state.UpdateChar();
                    }
                    else
                        state = _optimum[posPrev].State;

                    if (posPrev == cur - 1)
                    {
                        if (_optimum[cur].IsShortRep())
                            state.UpdateShortRep();
                        else
                            state.UpdateChar();
                    }
                    else
                    {
                        uint pos;
                        if (_optimum[cur].Prev1IsChar && _optimum[cur].Prev2)
                        {
                            posPrev = _optimum[cur].PosPrev2;
                            pos = _optimum[cur].BackPrev2;
                            state.UpdateRep();
                        }
                        else
                        {
                            pos = _optimum[cur].BackPrev;
                            if (pos < Base.kNumRepDistances)
                                state.UpdateRep();
                            else
                                state.UpdateMatch();
                        }

                        Optimal opt = _optimum[posPrev];
                        if (pos < Base.kNumRepDistances)
                        {
                            switch (pos)
                            {
                                case 0:
                                    reps[0] = opt.Backs0;
                                    reps[1] = opt.Backs1;
                                    reps[2] = opt.Backs2;
                                    reps[3] = opt.Backs3;
                                    break;
                                case 1:
                                    reps[0] = opt.Backs1;
                                    reps[1] = opt.Backs0;
                                    reps[2] = opt.Backs2;
                                    reps[3] = opt.Backs3;
                                    break;
                                case 2:
                                    reps[0] = opt.Backs2;
                                    reps[1] = opt.Backs0;
                                    reps[2] = opt.Backs1;
                                    reps[3] = opt.Backs3;
                                    break;
                                default:
                                    reps[0] = opt.Backs3;
                                    reps[1] = opt.Backs0;
                                    reps[2] = opt.Backs1;
                                    reps[3] = opt.Backs2;
                                    break;
                            }
                        }
                        else
                        {
                            reps[0] = pos - Base.kNumRepDistances;
                            reps[1] = opt.Backs0;
                            reps[2] = opt.Backs1;
                            reps[3] = opt.Backs2;
                        }
                    }

                    _optimum[cur].State = state;
                    _optimum[cur].Backs0 = reps[0];
                    _optimum[cur].Backs1 = reps[1];
                    _optimum[cur].Backs2 = reps[2];
                    _optimum[cur].Backs3 = reps[3];
                    uint curPrice = _optimum[cur].Price;

                    currentByte = _matchFinder.GetIndexByte(0 - 1);
                    matchByte = _matchFinder.GetIndexByte((int)(0 - reps[0] - 1 - 1));

                    posState = position & _posStateMask;

                    uint curAnd1Price = curPrice +
                                        _isMatch[(state.Index << Base.kNumPosStatesBitsMax) + posState].GetPrice0() +
                                        _literalEncoder.GetSubCoder(position, _matchFinder.GetIndexByte(0 - 2))
                                                       .GetPrice(!state.IsCharState(), matchByte, currentByte);

                    Optimal nextOptimum = _optimum[cur + 1];

                    bool nextIsChar = false;
                    if (curAnd1Price < nextOptimum.Price)
                    {
                        nextOptimum.Price = curAnd1Price;
                        nextOptimum.PosPrev = cur;
                        nextOptimum.MakeAsChar();
                        nextIsChar = true;
                    }

                    matchPrice = curPrice + _isMatch[(state.Index << Base.kNumPosStatesBitsMax) + posState].GetPrice1();
                    repMatchPrice = matchPrice + _isRep[state.Index].GetPrice1();

                    if (matchByte == currentByte &&
                        !(nextOptimum.PosPrev < cur && nextOptimum.BackPrev == 0))
                    {
                        uint shortRepPrice = repMatchPrice + GetRepLen1Price(state, posState);
                        if (shortRepPrice <= nextOptimum.Price)
                        {
                            nextOptimum.Price = shortRepPrice;
                            nextOptimum.PosPrev = cur;
                            nextOptimum.MakeAsShortRep();
                            nextIsChar = true;
                        }
                    }

                    uint numAvailableBytesFull = _matchFinder.GetNumAvailableBytes() + 1;
                    numAvailableBytesFull = Math.Min(kNumOpts - 1 - cur, numAvailableBytesFull);
                    numAvailableBytes = numAvailableBytesFull;

                    if (numAvailableBytes < 2)
                        continue;
                    if (numAvailableBytes > _numFastBytes)
                        numAvailableBytes = _numFastBytes;
                    if (!nextIsChar && matchByte != currentByte)
                    {
                        // try Literal + rep0
                        uint t = Math.Min(numAvailableBytesFull - 1, _numFastBytes);
                        uint lenTest2 = _matchFinder.GetMatchLen(0, reps[0], t);
                        if (lenTest2 >= 2)
                        {
                            Base.State state2 = state;
                            state2.UpdateChar();
                            uint posStateNext = (position + 1) & _posStateMask;
                            uint nextRepMatchPrice = curAnd1Price +
                                                     _isMatch[
                                                             (state2.Index << Base.kNumPosStatesBitsMax) + posStateNext]
                                                         .GetPrice1() +
                                                     _isRep[state2.Index].GetPrice1();
                            uint offset = cur + 1 + lenTest2;
                            while (lenEnd < offset)
                                _optimum[++lenEnd].Price = kInfinityPrice;
                            uint curAndLenPrice = nextRepMatchPrice + GetRepPrice(
                                0, lenTest2, state2, posStateNext);
                            Optimal optimum = _optimum[offset];
                            if (curAndLenPrice < optimum.Price)
                            {
                                optimum.Price = curAndLenPrice;
                                optimum.PosPrev = cur + 1;
                                optimum.BackPrev = 0;
                                optimum.Prev1IsChar = true;
                                optimum.Prev2 = false;
                            }
                        }
                    }

                    uint startLen = 2; // speed optimization

                    for (uint repIndex = 0; repIndex < Base.kNumRepDistances; repIndex++)
                    {
                        uint lenTest = _matchFinder.GetMatchLen(0 - 1, reps[repIndex], numAvailableBytes);
                        if (lenTest < 2)
                            continue;
                        uint lenTestTemp = lenTest;
                        do
                        {
                            while (lenEnd < cur + lenTest)
                                _optimum[++lenEnd].Price = kInfinityPrice;
                            uint curAndLenPrice = repMatchPrice + GetRepPrice(repIndex, lenTest, state, posState);
                            Optimal optimum = _optimum[cur + lenTest];
                            if (curAndLenPrice < optimum.Price)
                            {
                                optimum.Price = curAndLenPrice;
                                optimum.PosPrev = cur;
                                optimum.BackPrev = repIndex;
                                optimum.Prev1IsChar = false;
                            }
                        } while (--lenTest >= 2);

                        lenTest = lenTestTemp;

                        if (repIndex == 0)
                            startLen = lenTest + 1;

                        // if (_maxMode)
                        if (lenTest < numAvailableBytesFull)
                        {
                            uint t = Math.Min(numAvailableBytesFull - 1 - lenTest, _numFastBytes);
                            uint lenTest2 = _matchFinder.GetMatchLen((int)lenTest, reps[repIndex], t);
                            if (lenTest2 >= 2)
                            {
                                Base.State state2 = state;
                                state2.UpdateRep();
                                uint posStateNext = (position + lenTest) & _posStateMask;
                                uint curAndLenCharPrice =
                                    repMatchPrice + GetRepPrice(repIndex, lenTest, state, posState) +
                                    _isMatch[(state2.Index << Base.kNumPosStatesBitsMax) + posStateNext].GetPrice0() +
                                    _literalEncoder.GetSubCoder(position + lenTest,
                                                                _matchFinder.GetIndexByte((int)lenTest - 2))
                                                   .GetPrice(true,
                                                             _matchFinder.GetIndexByte((int)lenTest - 1
                                                                 - (int)(reps[repIndex] + 1)),
                                                             _matchFinder.GetIndexByte((int)lenTest - 1));
                                state2.UpdateChar();
                                posStateNext = (position + lenTest + 1) & _posStateMask;
                                uint nextMatchPrice = curAndLenCharPrice
                                                      + _isMatch[
                                                              (state2.Index << Base.kNumPosStatesBitsMax)
                                                              + posStateNext]
                                                          .GetPrice1();
                                uint nextRepMatchPrice = nextMatchPrice + _isRep[state2.Index].GetPrice1();

                                // for(; lenTest2 >= 2; lenTest2--)
                                {
                                    uint offset = lenTest + 1 + lenTest2;
                                    while (lenEnd < cur + offset)
                                        _optimum[++lenEnd].Price = kInfinityPrice;
                                    uint curAndLenPrice
                                        = nextRepMatchPrice + GetRepPrice(0, lenTest2, state2, posStateNext);
                                    Optimal optimum = _optimum[cur + offset];
                                    if (curAndLenPrice < optimum.Price)
                                    {
                                        optimum.Price = curAndLenPrice;
                                        optimum.PosPrev = cur + lenTest + 1;
                                        optimum.BackPrev = 0;
                                        optimum.Prev1IsChar = true;
                                        optimum.Prev2 = true;
                                        optimum.PosPrev2 = cur;
                                        optimum.BackPrev2 = repIndex;
                                    }
                                }
                            }
                        }
                    }

                    if (newLen > numAvailableBytes)
                    {
                        newLen = numAvailableBytes;
                        for (numDistancePairs = 0; newLen > _matchDistances[numDistancePairs]; numDistancePairs += 2)
                        {
                        }

                        _matchDistances[numDistancePairs] = newLen;
                        numDistancePairs += 2;
                    }

                    if (newLen >= startLen)
                    {
                        normalMatchPrice = matchPrice + _isRep[state.Index].GetPrice0();
                        while (lenEnd < cur + newLen)
                            _optimum[++lenEnd].Price = kInfinityPrice;

                        uint offs = 0;
                        while (startLen > _matchDistances[offs])
                            offs += 2;

                        for (uint lenTest = startLen; ; lenTest++)
                        {
                            uint curBack = _matchDistances[offs + 1];
                            uint curAndLenPrice = normalMatchPrice + GetPosLenPrice(curBack, lenTest, posState);
                            Optimal optimum = _optimum[cur + lenTest];
                            if (curAndLenPrice < optimum.Price)
                            {
                                optimum.Price = curAndLenPrice;
                                optimum.PosPrev = cur;
                                optimum.BackPrev = curBack + Base.kNumRepDistances;
                                optimum.Prev1IsChar = false;
                            }

                            if (lenTest == _matchDistances[offs])
                            {
                                if (lenTest < numAvailableBytesFull)
                                {
                                    uint t = Math.Min(numAvailableBytesFull - 1 - lenTest, _numFastBytes);
                                    uint lenTest2 = _matchFinder.GetMatchLen((int)lenTest, curBack, t);
                                    if (lenTest2 >= 2)
                                    {
                                        Base.State state2 = state;
                                        state2.UpdateMatch();
                                        uint posStateNext = (position + lenTest) & _posStateMask;
                                        uint curAndLenCharPrice = curAndLenPrice +
                                                                  _isMatch[
                                                                      (state2.Index << Base.kNumPosStatesBitsMax)
                                                                      + posStateNext].GetPrice0() +
                                                                  _literalEncoder.GetSubCoder(position + lenTest,
                                                                          _matchFinder.GetIndexByte((int)lenTest - 2))
                                                                      .GetPrice(true,
                                                                          _matchFinder.GetIndexByte(
                                                                              (int)lenTest - (int)(curBack + 1)
                                                                              - 1),
                                                                          _matchFinder.GetIndexByte(
                                                                              (int)lenTest - 1));
                                        state2.UpdateChar();
                                        posStateNext = (position + lenTest + 1) & _posStateMask;
                                        uint nextMatchPrice = curAndLenCharPrice
                                                              + _isMatch[
                                                                  (state2.Index << Base.kNumPosStatesBitsMax)
                                                                  + posStateNext].GetPrice1();
                                        uint nextRepMatchPrice = nextMatchPrice + _isRep[state2.Index].GetPrice1();

                                        uint offset = lenTest + 1 + lenTest2;
                                        while (lenEnd < cur + offset)
                                            _optimum[++lenEnd].Price = kInfinityPrice;
                                        curAndLenPrice = nextRepMatchPrice
                                                         + GetRepPrice(0, lenTest2, state2, posStateNext);
                                        optimum = _optimum[cur + offset];
                                        if (curAndLenPrice < optimum.Price)
                                        {
                                            optimum.Price = curAndLenPrice;
                                            optimum.PosPrev = cur + lenTest + 1;
                                            optimum.BackPrev = 0;
                                            optimum.Prev1IsChar = true;
                                            optimum.Prev2 = true;
                                            optimum.PosPrev2 = cur;
                                            optimum.BackPrev2 = curBack + Base.kNumRepDistances;
                                        }
                                    }
                                }

                                offs += 2;
                                if (offs == numDistancePairs)
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private static bool ChangePair(uint smallDist, uint bigDist)
        {
            const int kDif = 7;
            unchecked
            {
                return smallDist < (uint)1 << (32 - kDif) && bigDist >= smallDist << kDif;
            }
        }

        private void WriteEndMarker(uint posState)
        {
            if (!_writeEndMark)
                return;

            unchecked
            {
                _isMatch[(_state.Index << Base.kNumPosStatesBitsMax) + posState].Encode(_rangeEncoder, 1);
                _isRep[_state.Index].Encode(_rangeEncoder, 0);
                _state.UpdateMatch();
                const uint len = Base.kMatchMinLen;
                _lenEncoder.Encode(_rangeEncoder, len - Base.kMatchMinLen, posState);
                const uint posSlot = (1 << Base.kNumPosSlotBits) - 1;
                uint lenToPosState = Base.GetLenToPosState(len);
                _posSlotEncoder[lenToPosState].Encode(_rangeEncoder, posSlot);
                const int footerBits = 30;
                const uint posReduced = ((uint)1 << footerBits) - 1;
                _rangeEncoder.EncodeDirectBits(posReduced >> Base.kNumAlignBits, footerBits - Base.kNumAlignBits);
                _posAlignEncoder.ReverseEncode(_rangeEncoder, posReduced & Base.kAlignMask);
            }
        }

        private async Task WriteEndMarkerAsync(uint posState, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!_writeEndMark)
                return;

            unchecked
            {
                await _isMatch[(_state.Index << Base.kNumPosStatesBitsMax) + posState].EncodeAsync(_rangeEncoder, 1, token).ConfigureAwait(false);
                await _isRep[_state.Index].EncodeAsync(_rangeEncoder, 0, token).ConfigureAwait(false);
                _state.UpdateMatch();
                const uint len = Base.kMatchMinLen;
                await _lenEncoder.EncodeAsync(_rangeEncoder, len - Base.kMatchMinLen, posState, token).ConfigureAwait(false);
                const uint posSlot = (1 << Base.kNumPosSlotBits) - 1;
                uint lenToPosState = Base.GetLenToPosState(len);
                await _posSlotEncoder[lenToPosState].EncodeAsync(_rangeEncoder, posSlot, token).ConfigureAwait(false);
                const int footerBits = 30;
                const uint posReduced = ((uint)1 << footerBits) - 1;
                await _rangeEncoder.EncodeDirectBitsAsync(posReduced >> Base.kNumAlignBits, footerBits - Base.kNumAlignBits, token).ConfigureAwait(false);
                await _posAlignEncoder.ReverseEncodeAsync(_rangeEncoder, posReduced & Base.kAlignMask, token).ConfigureAwait(false);
            }
        }

        private void Flush(uint nowPos)
        {
            ReleaseMFStream();
            WriteEndMarker(nowPos & _posStateMask);
            _rangeEncoder.FlushData();
            _rangeEncoder.FlushStream();
        }

        private async Task FlushAsync(uint nowPos, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            ReleaseMFStream();
            await WriteEndMarkerAsync(nowPos & _posStateMask, token).ConfigureAwait(false);
            await _rangeEncoder.FlushDataAsync(token).ConfigureAwait(false);
            await _rangeEncoder.FlushStreamAsync(token).ConfigureAwait(false);
        }

        public void CodeOneBlock(out long inSize, out long outSize, out bool finished, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            inSize = 0;
            outSize = 0;
            finished = true;

            if (_inStream != null)
            {
                _matchFinder.SetStream(_inStream);
                _matchFinder.Init();
                _needReleaseMFStream = true;
                _inStream = null;
                token.ThrowIfCancellationRequested();
                if (_trainSize > 0)
                    _matchFinder.Skip(_trainSize);
            }

            if (_finished)
                return;
            _finished = true;

            unchecked
            {
                long progressPosValuePrev = _nowPos64;
                if (_nowPos64 == 0)
                {
                    token.ThrowIfCancellationRequested();
                    if (_matchFinder.GetNumAvailableBytes() == 0)
                    {
                        Flush((uint)_nowPos64);
                        return;
                    }

                    token.ThrowIfCancellationRequested();
                    ReadMatchDistances(out uint _, out uint _);
                    token.ThrowIfCancellationRequested();
                    uint posState = (uint)_nowPos64 & _posStateMask;
                    _isMatch[(_state.Index << Base.kNumPosStatesBitsMax) + posState].Encode(_rangeEncoder, 0);
                    token.ThrowIfCancellationRequested();
                    _state.UpdateChar();
                    token.ThrowIfCancellationRequested();
                    byte curByte = _matchFinder.GetIndexByte((int)(0 - _additionalOffset));
                    token.ThrowIfCancellationRequested();
                    _literalEncoder.GetSubCoder((uint)_nowPos64, _previousByte).Encode(_rangeEncoder, curByte);
                    _previousByte = curByte;
                    _additionalOffset--;
                    _nowPos64++;
                }

                token.ThrowIfCancellationRequested();
                if (_matchFinder.GetNumAvailableBytes() == 0)
                {
                    Flush((uint)_nowPos64);
                    return;
                }

                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    uint len = GetOptimum((uint)_nowPos64, out uint pos);

                    uint posState = (uint)_nowPos64 & _posStateMask;
                    uint complexState = (_state.Index << Base.kNumPosStatesBitsMax) + posState;

                    if (len == 1 && pos == 0xFFFFFFFF)
                    {
                        token.ThrowIfCancellationRequested();
                        _isMatch[complexState].Encode(_rangeEncoder, 0);
                        token.ThrowIfCancellationRequested();
                        byte curByte = _matchFinder.GetIndexByte((int)(0 - _additionalOffset));
                        LiteralEncoder.Encoder2 subCoder = _literalEncoder.GetSubCoder((uint)_nowPos64, _previousByte);
                        token.ThrowIfCancellationRequested();
                        if (!_state.IsCharState())
                        {
                            byte matchByte
                                = _matchFinder.GetIndexByte((int)(0 - _repDistances[0] - 1 - _additionalOffset));
                            subCoder.EncodeMatched(_rangeEncoder, matchByte, curByte);
                        }
                        else
                            subCoder.Encode(_rangeEncoder, curByte);
                        token.ThrowIfCancellationRequested();

                        _previousByte = curByte;
                        _state.UpdateChar();
                    }
                    else
                    {
                        token.ThrowIfCancellationRequested();
                        _isMatch[complexState].Encode(_rangeEncoder, 1);
                        if (pos < Base.kNumRepDistances)
                        {
                            token.ThrowIfCancellationRequested();
                            _isRep[_state.Index].Encode(_rangeEncoder, 1);
                            if (pos == 0)
                            {
                                token.ThrowIfCancellationRequested();
                                _isRepG0[_state.Index].Encode(_rangeEncoder, 0);
                                token.ThrowIfCancellationRequested();
                                _isRep0Long[complexState].Encode(_rangeEncoder, len == 1 ? 0 : (uint) 1);
                            }
                            else
                            {
                                token.ThrowIfCancellationRequested();
                                _isRepG0[_state.Index].Encode(_rangeEncoder, 1);
                                if (pos == 1)
                                {
                                    token.ThrowIfCancellationRequested();
                                    _isRepG1[_state.Index].Encode(_rangeEncoder, 0);
                                }
                                else
                                {
                                    token.ThrowIfCancellationRequested();
                                    _isRepG1[_state.Index].Encode(_rangeEncoder, 1);
                                    token.ThrowIfCancellationRequested();
                                    _isRepG2[_state.Index].Encode(_rangeEncoder, pos - 2);
                                }
                            }

                            if (len == 1)
                            {
                                token.ThrowIfCancellationRequested();
                                _state.UpdateShortRep();
                            }
                            else
                            {
                                token.ThrowIfCancellationRequested();
                                _repMatchLenEncoder.Encode(_rangeEncoder, len - Base.kMatchMinLen, posState);
                                token.ThrowIfCancellationRequested();
                                _state.UpdateRep();
                            }

                            token.ThrowIfCancellationRequested();
                            uint distance = _repDistances[pos];
                            if (pos != 0)
                            {
                                for (uint i = pos; i >= 1; i--)
                                    _repDistances[i] = _repDistances[i - 1];
                                _repDistances[0] = distance;
                            }
                        }
                        else
                        {
                            token.ThrowIfCancellationRequested();
                            _isRep[_state.Index].Encode(_rangeEncoder, 0);
                            token.ThrowIfCancellationRequested();
                            _state.UpdateMatch();
                            token.ThrowIfCancellationRequested();
                            _lenEncoder.Encode(_rangeEncoder, len - Base.kMatchMinLen, posState);
                            token.ThrowIfCancellationRequested();
                            pos -= Base.kNumRepDistances;
                            uint posSlot = GetPosSlot(pos);
                            uint lenToPosState = Base.GetLenToPosState(len);
                            token.ThrowIfCancellationRequested();
                            _posSlotEncoder[lenToPosState].Encode(_rangeEncoder, posSlot);

                            if (posSlot >= Base.kStartPosModelIndex)
                            {
                                int footerBits = (int)((posSlot >> 1) - 1);
                                uint baseVal = (2 | (posSlot & 1)) << footerBits;
                                uint posReduced = pos - baseVal;

                                if (posSlot < Base.kEndPosModelIndex)
                                {
                                    token.ThrowIfCancellationRequested();
                                    BitTreeEncoder.ReverseEncode(_posEncoders,
                                                                 baseVal - posSlot - 1, _rangeEncoder, footerBits,
                                                                 posReduced);
                                }
                                else
                                {
                                    token.ThrowIfCancellationRequested();
                                    _rangeEncoder.EncodeDirectBits(posReduced >> Base.kNumAlignBits,
                                                                   footerBits - Base.kNumAlignBits);
                                    token.ThrowIfCancellationRequested();
                                    _posAlignEncoder.ReverseEncode(_rangeEncoder, posReduced & Base.kAlignMask);
                                    _alignPriceCount++;
                                }
                            }

                            token.ThrowIfCancellationRequested();
                            uint distance = pos;
                            for (uint i = Base.kNumRepDistances - 1; i >= 1; i--)
                                _repDistances[i] = _repDistances[i - 1];
                            _repDistances[0] = distance;
                            _matchPriceCount++;
                        }

                        token.ThrowIfCancellationRequested();
                        _previousByte = _matchFinder.GetIndexByte((int)(len - 1 - _additionalOffset));
                    }

                    _additionalOffset -= len;
                    _nowPos64 += len;
                    if (_additionalOffset == 0)
                    {
                        token.ThrowIfCancellationRequested();
                        // if (!_fastMode)
                        if (_matchPriceCount >= 1 << 7)
                            FillDistancesPrices(token);

                        if (_alignPriceCount >= Base.kAlignTableSize)
                            FillAlignPrices(token);

                        token.ThrowIfCancellationRequested();
                        inSize = _nowPos64;
                        outSize = _rangeEncoder.GetProcessedSizeAdd();
                        token.ThrowIfCancellationRequested();
                        if (_matchFinder.GetNumAvailableBytes() == 0)
                        {
                            Flush((uint)_nowPos64);
                            return;
                        }

                        if (_nowPos64 - progressPosValuePrev >= 1 << 12)
                        {
                            _finished = false;
                            finished = false;
                            return;
                        }
                    }
                }
            }
        }

        public async Task<Tuple<long, long, bool>> CodeOneBlockAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long inSize = 0;
            long outSize = 0;

            if (_inStream != null)
            {
                _matchFinder.SetStream(_inStream);
                await _matchFinder.InitAsync(token).ConfigureAwait(false);
                _needReleaseMFStream = true;
                _inStream = null;
                token.ThrowIfCancellationRequested();
                if (_trainSize > 0)
                    await _matchFinder.SkipAsync(_trainSize, token).ConfigureAwait(false);
            }

            if (_finished)
                return new Tuple<long, long, bool>(inSize, outSize, true);
            _finished = true;

            unchecked
            {
                long progressPosValuePrev = _nowPos64;
                if (_nowPos64 == 0)
                {
                    token.ThrowIfCancellationRequested();
                    if (_matchFinder.GetNumAvailableBytes() == 0)
                    {
                        await FlushAsync((uint)_nowPos64, token).ConfigureAwait(false);
                        return new Tuple<long, long, bool> (inSize, outSize, true);
                    }
                    
                    await ReadMatchDistancesAsync(token).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    uint posState = (uint)_nowPos64 & _posStateMask;
                    await _isMatch[(_state.Index << Base.kNumPosStatesBitsMax) + posState].EncodeAsync(_rangeEncoder, 0, token).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    _state.UpdateChar();
                    token.ThrowIfCancellationRequested();
                    byte curByte = _matchFinder.GetIndexByte((int)(0 - _additionalOffset));
                    await _literalEncoder.GetSubCoder((uint)_nowPos64, _previousByte).EncodeAsync(_rangeEncoder, curByte, token).ConfigureAwait(false);
                    _previousByte = curByte;
                    _additionalOffset--;
                    _nowPos64++;
                }

                token.ThrowIfCancellationRequested();
                if (_matchFinder.GetNumAvailableBytes() == 0)
                {
                    await FlushAsync((uint)_nowPos64, token).ConfigureAwait(false);
                    return new Tuple<long, long, bool>(inSize, outSize, true);
                }

                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    (uint len, uint pos) = await GetOptimumAsync((uint)_nowPos64, token).ConfigureAwait(false);

                    uint posState = (uint)_nowPos64 & _posStateMask;
                    uint complexState = (_state.Index << Base.kNumPosStatesBitsMax) + posState;

                    if (len == 1 && pos == 0xFFFFFFFF)
                    {
                        await _isMatch[complexState].EncodeAsync(_rangeEncoder, 0, token).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        byte curByte = _matchFinder.GetIndexByte((int)(0 - _additionalOffset));
                        LiteralEncoder.Encoder2 subCoder = _literalEncoder.GetSubCoder((uint)_nowPos64, _previousByte);
                        token.ThrowIfCancellationRequested();
                        if (!_state.IsCharState())
                        {
                            byte matchByte
                                = _matchFinder.GetIndexByte((int)(0 - _repDistances[0] - 1 - _additionalOffset));
                            await subCoder.EncodeMatchedAsync(_rangeEncoder, matchByte, curByte, token).ConfigureAwait(false);
                        }
                        else
                            await subCoder.EncodeAsync(_rangeEncoder, curByte, token).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();

                        _previousByte = curByte;
                        _state.UpdateChar();
                    }
                    else
                    {
                        await _isMatch[complexState].EncodeAsync(_rangeEncoder, 1, token).ConfigureAwait(false);
                        if (pos < Base.kNumRepDistances)
                        {
                            await _isRep[_state.Index].EncodeAsync(_rangeEncoder, 1, token).ConfigureAwait(false);
                            if (pos == 0)
                            {
                                await _isRepG0[_state.Index].EncodeAsync(_rangeEncoder, 0, token).ConfigureAwait(false);
                                await _isRep0Long[complexState].EncodeAsync(_rangeEncoder, len == 1 ? 0 : (uint)1, token).ConfigureAwait(false);
                            }
                            else
                            {
                                await _isRepG0[_state.Index].EncodeAsync(_rangeEncoder, 1, token).ConfigureAwait(false);
                                if (pos == 1)
                                {
                                    await _isRepG1[_state.Index].EncodeAsync(_rangeEncoder, 0, token).ConfigureAwait(false);
                                }
                                else
                                {
                                    await _isRepG1[_state.Index].EncodeAsync(_rangeEncoder, 1, token).ConfigureAwait(false);
                                    await _isRepG2[_state.Index].EncodeAsync(_rangeEncoder, pos - 2, token).ConfigureAwait(false);
                                }
                            }

                            if (len == 1)
                            {
                                token.ThrowIfCancellationRequested();
                                _state.UpdateShortRep();
                            }
                            else
                            {
                                await _repMatchLenEncoder.EncodeAsync(_rangeEncoder, len - Base.kMatchMinLen, posState, token).ConfigureAwait(false);
                                token.ThrowIfCancellationRequested();
                                _state.UpdateRep();
                            }

                            token.ThrowIfCancellationRequested();
                            uint distance = _repDistances[pos];
                            if (pos != 0)
                            {
                                for (uint i = pos; i >= 1; i--)
                                    _repDistances[i] = _repDistances[i - 1];
                                _repDistances[0] = distance;
                            }
                        }
                        else
                        {
                            await _isRep[_state.Index].EncodeAsync(_rangeEncoder, 0, token).ConfigureAwait(false);
                            token.ThrowIfCancellationRequested();
                            _state.UpdateMatch();
                            await _lenEncoder.EncodeAsync(_rangeEncoder, len - Base.kMatchMinLen, posState, token).ConfigureAwait(false);
                            token.ThrowIfCancellationRequested();
                            pos -= Base.kNumRepDistances;
                            uint posSlot = GetPosSlot(pos);
                            uint lenToPosState = Base.GetLenToPosState(len);
                            await _posSlotEncoder[lenToPosState].EncodeAsync(_rangeEncoder, posSlot, token).ConfigureAwait(false);

                            if (posSlot >= Base.kStartPosModelIndex)
                            {
                                int footerBits = (int)((posSlot >> 1) - 1);
                                uint baseVal = (2 | (posSlot & 1)) << footerBits;
                                uint posReduced = pos - baseVal;

                                if (posSlot < Base.kEndPosModelIndex)
                                {
                                    await BitTreeEncoder.ReverseEncodeAsync(_posEncoders,
                                                                            baseVal - posSlot - 1, _rangeEncoder, footerBits,
                                                                            posReduced, token).ConfigureAwait(false);
                                }
                                else
                                {
                                    token.ThrowIfCancellationRequested();
                                    await _rangeEncoder.EncodeDirectBitsAsync(posReduced >> Base.kNumAlignBits,
                                                                              footerBits - Base.kNumAlignBits, token).ConfigureAwait(false);
                                    await _posAlignEncoder.ReverseEncodeAsync(_rangeEncoder, posReduced & Base.kAlignMask, token).ConfigureAwait(false);
                                    _alignPriceCount++;
                                }
                            }

                            token.ThrowIfCancellationRequested();
                            uint distance = pos;
                            for (uint i = Base.kNumRepDistances - 1; i >= 1; i--)
                                _repDistances[i] = _repDistances[i - 1];
                            _repDistances[0] = distance;
                            _matchPriceCount++;
                        }

                        token.ThrowIfCancellationRequested();
                        _previousByte = _matchFinder.GetIndexByte((int)(len - 1 - _additionalOffset));
                    }

                    _additionalOffset -= len;
                    _nowPos64 += len;
                    if (_additionalOffset == 0)
                    {
                        token.ThrowIfCancellationRequested();
                        // if (!_fastMode)
                        if (_matchPriceCount >= 1 << 7)
                            FillDistancesPrices(token);

                        if (_alignPriceCount >= Base.kAlignTableSize)
                            FillAlignPrices(token);

                        token.ThrowIfCancellationRequested();
                        inSize = _nowPos64;
                        outSize = _rangeEncoder.GetProcessedSizeAdd();
                        token.ThrowIfCancellationRequested();
                        if (_matchFinder.GetNumAvailableBytes() == 0)
                        {
                            await FlushAsync((uint)_nowPos64, token).ConfigureAwait(false);
                            return new Tuple<long, long, bool>(inSize, outSize, true);
                        }

                        if (_nowPos64 - progressPosValuePrev >= 1 << 12)
                        {
                            _finished = false;
                            return new Tuple<long, long, bool>(inSize, outSize, false);
                        }
                    }
                }
            }
        }

        private void ReleaseMFStream()
        {
            if (_matchFinder != null && _needReleaseMFStream)
            {
                _matchFinder.ReleaseStream();
                _needReleaseMFStream = false;
            }
        }

        private void SetOutStream(Stream outStream)
        { _rangeEncoder.SetStream(outStream); }

        private void ReleaseOutStream()
        { _rangeEncoder.ReleaseStream(); }

        private void ReleaseStreams()
        {
            ReleaseMFStream();
            ReleaseOutStream();
        }

        private void SetStreams(Stream inStream, Stream outStream,
                long inSize, long outSize, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            _inStream = inStream;
            _finished = false;
            Create();
            SetOutStream(outStream);
            Init(token);

            // if (!_fastMode)
            {
                FillDistancesPrices(token);
                FillAlignPrices(token);
            }
            token.ThrowIfCancellationRequested();

            unchecked
            {
                _lenEncoder.SetTableSize(_numFastBytes - (Base.kMatchMinLen - 1));
                _lenEncoder.UpdateTables((uint)1 << _posStateBits);
                _repMatchLenEncoder.SetTableSize(_numFastBytes - (Base.kMatchMinLen - 1));
                _repMatchLenEncoder.UpdateTables((uint)1 << _posStateBits);
            }

            _nowPos64 = 0;
        }

        public void Code(Stream inStream, Stream outStream,
                         long inSize, long outSize, ICodeProgress progress)
        {
            Chummer.Utils.SafelyRunSynchronously(() => CodeCoreAsync(true, inStream, outStream, inSize, outSize, progress, null, CancellationToken.None), CancellationToken.None);
        }

        public Task CodeAsync(Stream inStream, Stream outStream,
                         long inSize, long outSize, IAsyncCodeProgress progress, CancellationToken token = default)
        {
            return CodeCoreAsync(false, inStream, outStream, inSize, outSize, null, progress, token);
        }

        private async Task CodeCoreAsync(bool blnSync, Stream inStream, Stream outStream,
                         long inSize, long outSize, ICodeProgress progress, IAsyncCodeProgress progressAsync, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            _needReleaseMFStream = false;
            try
            {
                SetStreams(inStream, outStream, inSize, outSize, token);
                while (true)
                {
                    long processedInSize;
                    long processedOutSize;
                    bool finished;
                    if (blnSync)
                        CodeOneBlock(out processedInSize, out processedOutSize, out finished, token);
                    else
                        (processedInSize, processedOutSize, finished)
                            = await CodeOneBlockAsync(token).ConfigureAwait(false);
                    if (finished)
                        return;
                    token.ThrowIfCancellationRequested();
                    if (blnSync)
                    {
                        progress?.SetProgress(processedInSize, processedOutSize);
                    }
                    else if (progressAsync != null)
                    {
                        await progressAsync.SetProgressAsync(processedInSize, processedOutSize, token).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                ReleaseStreams();
            }
        }

        private const int kPropSize = 5;
        private readonly byte[] myProperties = new byte[kPropSize];

        public void WriteCoderProperties(Stream outStream)
        {
            unchecked
            {
                myProperties[0] = (byte)((_posStateBits * 5 + _numLiteralPosStateBits) * 9 + _numLiteralContextBits);
                for (int i = 0; i < 4; i++)
                    myProperties[1 + i] = (byte)((_dictionarySize >> (8 * i)) & 0xFF);
                outStream.Write(myProperties, 0, kPropSize);
            }
        }

        public Task WriteCoderPropertiesAsync(Stream outStream, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            unchecked
            {
                myProperties[0] = (byte)((_posStateBits * 5 + _numLiteralPosStateBits) * 9 + _numLiteralContextBits);
                for (int i = 0; i < 4; i++)
                    myProperties[1 + i] = (byte)((_dictionarySize >> (8 * i)) & 0xFF);
                return outStream.WriteAsync(myProperties, 0, kPropSize, token);
            }
        }

        private readonly uint[] tempPrices = new uint[Base.kNumFullDistances];
        private uint _matchPriceCount;

        private void FillDistancesPrices(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            unchecked
            {
                for (uint i = Base.kStartPosModelIndex; i < Base.kNumFullDistances; i++)
                {
                    token.ThrowIfCancellationRequested();
                    uint posSlot = GetPosSlot(i);
                    int footerBits = (int)((posSlot >> 1) - 1);
                    uint baseVal = (2 | (posSlot & 1)) << footerBits;
                    tempPrices[i] = BitTreeEncoder.ReverseGetPrice(_posEncoders,
                                                                   baseVal - posSlot - 1, footerBits, i - baseVal);
                }

                for (uint lenToPosState = 0; lenToPosState < Base.kNumLenToPosStates; lenToPosState++)
                {
                    token.ThrowIfCancellationRequested();
                    uint posSlot;
                    BitTreeEncoder encoder = _posSlotEncoder[lenToPosState];

                    uint st = lenToPosState << Base.kNumPosSlotBits;
                    for (posSlot = 0; posSlot < _distTableSize; posSlot++)
                    {
                        token.ThrowIfCancellationRequested();
                        _posSlotPrices[st + posSlot] = encoder.GetPrice(posSlot);
                    }

                    for (posSlot = Base.kEndPosModelIndex; posSlot < _distTableSize; posSlot++)
                    {
                        token.ThrowIfCancellationRequested();
                        _posSlotPrices[st + posSlot] += ((posSlot >> 1) - (1 + Base.kNumAlignBits))
                                                        << BitEncoder.kNumBitPriceShiftBits;
                    }

                    uint st2 = lenToPosState * Base.kNumFullDistances;
                    uint i;
                    for (i = 0; i < Base.kStartPosModelIndex; i++)
                    {
                        token.ThrowIfCancellationRequested();
                        _distancesPrices[st2 + i] = _posSlotPrices[st + i];
                    }

                    for (; i < Base.kNumFullDistances; i++)
                    {
                        token.ThrowIfCancellationRequested();
                        _distancesPrices[st2 + i] = _posSlotPrices[st + GetPosSlot(i)] + tempPrices[i];
                    }
                }
            }

            _matchPriceCount = 0;
        }

        private void FillAlignPrices(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            for (uint i = 0; i < Base.kAlignTableSize; i++)
            {
                token.ThrowIfCancellationRequested();
                _alignPrices[i] = _posAlignEncoder.ReverseGetPrice(i);
            }

            _alignPriceCount = 0;
        }

        public void SetCoderProperties(CoderPropID[] propIDs, object[] properties)
        {
            for (uint i = 0; i < properties.Length; i++)
            {
                object prop = properties[i];
                switch (propIDs[i])
                {
                    case CoderPropID.NumFastBytes:
                        {
                            if (!(prop is int numFastBytes))
                                throw new InvalidParamException();
                            if (numFastBytes < 5 || numFastBytes > Base.kMatchMaxLen)
                                throw new InvalidParamException();
                            _numFastBytes = (uint)numFastBytes;
                            break;
                        }
                    case CoderPropID.Algorithm:
                        {
                            /*
                            if (!(prop is Int32))
                                throw new InvalidParamException();
                            Int32 maximize = (Int32)prop;
                            _fastMode = (maximize == 0);
                            _maxMode = (maximize >= 2);
                            */
                            break;
                        }
                    case CoderPropID.MatchFinder:
                        {
                            EMatchFinderType matchFinderIndexPrev = _matchFinderType;
                            if (!(prop is EMatchFinderType id))
                            {
                                if (!(prop is string needle))
                                    throw new InvalidParamException();
                                if (!Enum.TryParse(needle, true, out id))
                                    throw new InvalidParamException();
                            }
                            _matchFinderType = id;
                            if (_matchFinder != null && matchFinderIndexPrev != _matchFinderType)
                            {
                                _dictionarySizePrev = 0xFFFFFFFF;
                                _matchFinder = null;
                            }
                            break;
                        }
                    case CoderPropID.DictionarySize:
                        {
                            const int kDicLogSizeMaxCompress = 30;
                            if (!(prop is int dictionarySize))
                                throw new InvalidParamException();
                            unchecked
                            {
                                if (dictionarySize < (uint)(1 << Base.kDicLogSizeMin) ||
                                    dictionarySize > (uint)(1 << kDicLogSizeMaxCompress))
                                    throw new InvalidParamException();
                                _dictionarySize = (uint)dictionarySize;
                                int dicLogSize;
                                for (dicLogSize = 0; dicLogSize < (uint)kDicLogSizeMaxCompress; dicLogSize++)
                                    if (dictionarySize <= (uint)1 << dicLogSize)
                                        break;
                                _distTableSize = (uint)dicLogSize * 2;
                            }

                            break;
                        }
                    case CoderPropID.PosStateBits:
                        {
                            if (!(prop is int bits))
                                throw new InvalidParamException();
                            if (bits < 0 || bits > (uint)Base.kNumPosStatesBitsEncodingMax)
                                throw new InvalidParamException();
                            _posStateBits = bits;
                            unchecked
                            {
                                _posStateMask = ((uint)1 << _posStateBits) - 1;
                            }

                            break;
                        }
                    case CoderPropID.LitPosBits:
                        {
                            if (!(prop is int bits))
                                throw new InvalidParamException();
                            if (bits < 0 || bits > Base.kNumLitPosStatesBitsEncodingMax)
                                throw new InvalidParamException();
                            _numLiteralPosStateBits = bits;
                            break;
                        }
                    case CoderPropID.LitContextBits:
                        {
                            if (!(prop is int bits))
                                throw new InvalidParamException();
                            if (bits < 0 || bits > Base.kNumLitContextBitsMax)
                                throw new InvalidParamException();
                            _numLiteralContextBits = bits;
                            break;
                        }
                    case CoderPropID.EndMarker:
                        {
                            if (!(prop is bool isMark))
                                throw new InvalidParamException();
                            SetWriteEndMarkerMode(isMark);
                            break;
                        }
                    default:
                        throw new InvalidParamException();
                }
            }
        }

        private uint _trainSize;

        [CLSCompliant(false)]
        public void SetTrainSize(uint trainSize)
        {
            _trainSize = trainSize;
        }
    }
}
