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
// LzmaDecoder.cs

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Chummer;
using SevenZip.Compression.LZ;
using SevenZip.Compression.RangeCoder;

namespace SevenZip.Compression.LZMA
{
    public class Decoder : ICoder, ISetDecoderProperties // ,System.IO.Stream
    {
        private sealed class LenDecoder
        {
            private BitDecoder m_Choice;
            private BitDecoder m_Choice2;
            private readonly BitTreeDecoder[] m_LowCoder = new BitTreeDecoder[Base.kNumPosStatesMax];
            private readonly BitTreeDecoder[] m_MidCoder = new BitTreeDecoder[Base.kNumPosStatesMax];
            private readonly BitTreeDecoder m_HighCoder = new BitTreeDecoder(Base.kNumHighLenBits);
            private uint m_NumPosStates;

            public void Create(uint numPosStates)
            {
                for (uint posState = m_NumPosStates; posState < numPosStates; posState++)
                {
                    m_LowCoder[posState] = new BitTreeDecoder(Base.kNumLowLenBits);
                    m_MidCoder[posState] = new BitTreeDecoder(Base.kNumMidLenBits);
                }
                m_NumPosStates = numPosStates;
            }

            public void Init()
            {
                m_Choice.Init();
                for (uint posState = 0; posState < m_NumPosStates; posState++)
                {
                    m_LowCoder[posState].Init();
                    m_MidCoder[posState].Init();
                }
                m_Choice2.Init();
                m_HighCoder.Init();
            }

            public uint Decode(RangeCoder.Decoder rangeDecoder, uint posState)
            {
                if (m_Choice.Decode(rangeDecoder) == 0)
                    return m_LowCoder[posState].Decode(rangeDecoder);
                uint symbol = Base.kNumLowLenSymbols;
                if (m_Choice2.Decode(rangeDecoder) == 0)
                    symbol += m_MidCoder[posState].Decode(rangeDecoder);
                else
                {
                    symbol += Base.kNumMidLenSymbols;
                    symbol += m_HighCoder.Decode(rangeDecoder);
                }
                return symbol;
            }
        }

        private sealed class LiteralDecoder
        {
            private struct Decoder2
            {
                private BitDecoder[] m_Decoders;

                public void Create()
                { m_Decoders = new BitDecoder[0x300]; }

                public void Init()
                { for (int i = 0; i < 0x300; i++) m_Decoders[i].Init(); }

                public byte DecodeNormal(RangeCoder.Decoder rangeDecoder)
                {
                    uint symbol = 1;
                    unchecked
                    {
                        do
                            symbol = (symbol << 1) | m_Decoders[symbol].Decode(rangeDecoder);
                        while (symbol < 0x100);
                        return (byte)symbol;
                    }
                }

                public byte DecodeWithMatchByte(RangeCoder.Decoder rangeDecoder, byte matchByte)
                {
                    unchecked
                    {
                        uint symbol = 1;
                        do
                        {
                            uint matchBit = (uint)(matchByte >> 7) & 1;
                            matchByte <<= 1;
                            uint bit = m_Decoders[((1 + matchBit) << 8) + symbol].Decode(rangeDecoder);
                            symbol = (symbol << 1) | bit;
                            if (matchBit != bit)
                            {
                                while (symbol < 0x100)
                                    symbol = (symbol << 1) | m_Decoders[symbol].Decode(rangeDecoder);
                                break;
                            }
                        } while (symbol < 0x100);

                        return (byte)symbol;
                    }
                }
            }

            private Decoder2[] m_Coders;
            private int m_NumPrevBits;
            private int m_NumPosBits;
            private uint m_PosMask;

            public void Create(int numPosBits, int numPrevBits)
            {
                if (m_Coders != null && m_NumPrevBits == numPrevBits &&
                    m_NumPosBits == numPosBits)
                    return;
                unchecked
                {
                    m_NumPosBits = numPosBits;
                    m_PosMask = ((uint)1 << numPosBits) - 1;
                    m_NumPrevBits = numPrevBits;
                    uint numStates = (uint)1 << (m_NumPrevBits + m_NumPosBits);
                    m_Coders = new Decoder2[numStates];
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

            private uint GetState(uint pos, byte prevByte)
            {
                unchecked
                {
                    return ((pos & m_PosMask) << m_NumPrevBits) + (uint)(prevByte >> (8 - m_NumPrevBits));
                }
            }

            public byte DecodeNormal(RangeCoder.Decoder rangeDecoder, uint pos, byte prevByte)
            { return m_Coders[GetState(pos, prevByte)].DecodeNormal(rangeDecoder); }

            public byte DecodeWithMatchByte(RangeCoder.Decoder rangeDecoder, uint pos, byte prevByte, byte matchByte)
            { return m_Coders[GetState(pos, prevByte)].DecodeWithMatchByte(rangeDecoder, matchByte); }
        }

        private readonly OutWindow m_OutWindow = new OutWindow();
        private readonly RangeCoder.Decoder m_RangeDecoder = new RangeCoder.Decoder();

        private readonly BitDecoder[] m_IsMatchDecoders = new BitDecoder[Base.kNumStates << Base.kNumPosStatesBitsMax];
        private readonly BitDecoder[] m_IsRepDecoders = new BitDecoder[Base.kNumStates];
        private readonly BitDecoder[] m_IsRepG0Decoders = new BitDecoder[Base.kNumStates];
        private readonly BitDecoder[] m_IsRepG1Decoders = new BitDecoder[Base.kNumStates];
        private readonly BitDecoder[] m_IsRepG2Decoders = new BitDecoder[Base.kNumStates];
        private readonly BitDecoder[] m_IsRep0LongDecoders = new BitDecoder[Base.kNumStates << Base.kNumPosStatesBitsMax];

        private readonly BitTreeDecoder[] m_PosSlotDecoder = new BitTreeDecoder[Base.kNumLenToPosStates];
        private readonly BitDecoder[] m_PosDecoders = new BitDecoder[Base.kNumFullDistances - Base.kEndPosModelIndex];

        private readonly BitTreeDecoder m_PosAlignDecoder = new BitTreeDecoder(Base.kNumAlignBits);

        private readonly LenDecoder m_LenDecoder = new LenDecoder();
        private readonly LenDecoder m_RepLenDecoder = new LenDecoder();

        private readonly LiteralDecoder m_LiteralDecoder = new LiteralDecoder();

        private uint m_DictionarySize;
        private uint m_DictionarySizeCheck;

        private uint m_PosStateMask;

        public Decoder()
        {
            m_DictionarySize = 0xFFFFFFFF;
            for (int i = 0; i < Base.kNumLenToPosStates; i++)
                m_PosSlotDecoder[i] = new BitTreeDecoder(Base.kNumPosSlotBits);
        }

        private void SetDictionarySize(uint dictionarySize)
        {
            if (m_DictionarySize != dictionarySize)
            {
                m_DictionarySize = dictionarySize;
                m_DictionarySizeCheck = Math.Max(m_DictionarySize, 1);
                uint blockSize = Math.Max(m_DictionarySizeCheck, 1 << 12);
                m_OutWindow.Create(blockSize);
            }
        }

        private void SetLiteralProperties(int lp, int lc)
        {
            if (lp > 8)
                throw new InvalidParamException();
            if (lc > 8)
                throw new InvalidParamException();
            m_LiteralDecoder.Create(lp, lc);
        }

        private void SetPosBitsProperties(int pb)
        {
            if (pb > Base.kNumPosStatesBitsMax)
                throw new InvalidParamException();
            unchecked
            {
                uint numPosStates = (uint)1 << pb;
                m_LenDecoder.Create(numPosStates);
                m_RepLenDecoder.Create(numPosStates);
                m_PosStateMask = numPosStates - 1;
            }
        }

        private bool _solid;

        private void Init(Stream inStream, Stream outStream)
        {
            m_RangeDecoder.Init(inStream);
            m_OutWindow.Init(outStream, _solid);

            uint i;
            for (i = 0; i < Base.kNumStates; i++)
            {
                unchecked
                {
                    for (uint j = 0; j <= m_PosStateMask; j++)
                    {
                        uint index = (i << Base.kNumPosStatesBitsMax) + j;
                        m_IsMatchDecoders[index].Init();
                        m_IsRep0LongDecoders[index].Init();
                    }
                }

                m_IsRepDecoders[i].Init();
                m_IsRepG0Decoders[i].Init();
                m_IsRepG1Decoders[i].Init();
                m_IsRepG2Decoders[i].Init();
            }

            m_LiteralDecoder.Init();
            for (i = 0; i < Base.kNumLenToPosStates; i++)
                m_PosSlotDecoder[i].Init();
            // m_PosSpecDecoder.Init();
            for (i = 0; i < Base.kNumFullDistances - Base.kEndPosModelIndex; i++)
                m_PosDecoders[i].Init();

            m_LenDecoder.Init();
            m_RepLenDecoder.Init();
            m_PosAlignDecoder.Init();
        }

        private async Task InitAsync(Stream inStream, Stream outStream, CancellationToken token = default)
        {
            await m_RangeDecoder.InitAsync(inStream, token).ConfigureAwait(false);
            await m_OutWindow.InitAsync(outStream, _solid, token).ConfigureAwait(false);

            uint i;
            for (i = 0; i < Base.kNumStates; i++)
            {
                unchecked
                {
                    for (uint j = 0; j <= m_PosStateMask; j++)
                    {
                        uint index = (i << Base.kNumPosStatesBitsMax) + j;
                        m_IsMatchDecoders[index].Init();
                        m_IsRep0LongDecoders[index].Init();
                    }
                }

                m_IsRepDecoders[i].Init();
                m_IsRepG0Decoders[i].Init();
                m_IsRepG1Decoders[i].Init();
                m_IsRepG2Decoders[i].Init();
            }

            m_LiteralDecoder.Init();
            for (i = 0; i < Base.kNumLenToPosStates; i++)
                m_PosSlotDecoder[i].Init();
            // m_PosSpecDecoder.Init();
            for (i = 0; i < Base.kNumFullDistances - Base.kEndPosModelIndex; i++)
                m_PosDecoders[i].Init();

            m_LenDecoder.Init();
            m_RepLenDecoder.Init();
            m_PosAlignDecoder.Init();
        }

        public void Code(Stream inStream, Stream outStream,
                         long inSize, long outSize, ICodeProgress progress)
        {
            Utils.SafelyRunSynchronously(() => CodeCoreAsync(true, inStream, outStream, inSize, outSize, progress, null, CancellationToken.None));
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
            if (blnSync)
                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                Init(inStream, outStream);
            else
                await InitAsync(inStream, outStream, token).ConfigureAwait(false);

            Base.State state = new Base.State();
            state.Init();
            try
            {
                token.ThrowIfCancellationRequested();
                unchecked
                {
                    uint rep0 = 0, rep1 = 0, rep2 = 0, rep3 = 0;
                    ulong nowPos64 = 0;
                    ulong outSize64 = (ulong)outSize;
                    if (nowPos64 < outSize64)
                    {
                        if (m_IsMatchDecoders[state.Index << Base.kNumPosStatesBitsMax].Decode(m_RangeDecoder) != 0)
                            throw new DataErrorException();
                        state.UpdateChar();
                        byte b = m_LiteralDecoder.DecodeNormal(m_RangeDecoder, 0, 0);
                        if (blnSync)
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            m_OutWindow.PutByte(b);
                        else
                            await m_OutWindow.PutByteAsync(b, token).ConfigureAwait(false);
                        nowPos64++;
                    }

                    token.ThrowIfCancellationRequested();
                    while (nowPos64 < outSize64)
                    {
                        token.ThrowIfCancellationRequested();
                        // UInt64 next = Math.Min(nowPos64 + (1 << 18), outSize64);
                        // while(nowPos64 < next)
                        {
                            uint posState = (uint)nowPos64 & m_PosStateMask;
                            if (m_IsMatchDecoders[(state.Index << Base.kNumPosStatesBitsMax) + posState]
                                    .Decode(m_RangeDecoder) == 0)
                            {
                                byte prevByte = m_OutWindow.GetByte(0);
                                byte b = !state.IsCharState()
                                    ? m_LiteralDecoder.DecodeWithMatchByte(m_RangeDecoder,
                                        (uint)nowPos64, prevByte,
                                        m_OutWindow.GetByte(rep0))
                                    : m_LiteralDecoder.DecodeNormal(m_RangeDecoder, (uint)nowPos64, prevByte);
                                token.ThrowIfCancellationRequested();
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    m_OutWindow.PutByte(b);
                                else
                                    await m_OutWindow.PutByteAsync(b, token).ConfigureAwait(false);
                                state.UpdateChar();
                                nowPos64++;
                            }
                            else
                            {
                                uint len;
                                if (m_IsRepDecoders[state.Index].Decode(m_RangeDecoder) == 1)
                                {
                                    if (m_IsRepG0Decoders[state.Index].Decode(m_RangeDecoder) == 0)
                                    {
                                        if (m_IsRep0LongDecoders[(state.Index << Base.kNumPosStatesBitsMax) + posState]
                                                .Decode(m_RangeDecoder) == 0)
                                        {
                                            state.UpdateShortRep();
                                            token.ThrowIfCancellationRequested();
                                            if (blnSync)
                                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                m_OutWindow.PutByte(m_OutWindow.GetByte(rep0));
                                            else
                                                await m_OutWindow.PutByteAsync(m_OutWindow.GetByte(rep0), token).ConfigureAwait(false);
                                            nowPos64++;
                                            if (blnSync)
                                            {
                                                progress?.SetProgress((long)outSize64, (long)nowPos64);
                                            }
                                            else if (progressAsync != null)
                                            {
                                                await progressAsync.SetProgressAsync((long)outSize64, (long)nowPos64, token).ConfigureAwait(false);
                                            }
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        uint distance;
                                        if (m_IsRepG1Decoders[state.Index].Decode(m_RangeDecoder) == 0)
                                        {
                                            distance = rep1;
                                        }
                                        else
                                        {
                                            if (m_IsRepG2Decoders[state.Index].Decode(m_RangeDecoder) == 0)
                                                distance = rep2;
                                            else
                                            {
                                                distance = rep3;
                                                rep3 = rep2;
                                            }

                                            rep2 = rep1;
                                        }

                                        rep1 = rep0;
                                        rep0 = distance;
                                    }

                                    len = m_RepLenDecoder.Decode(m_RangeDecoder, posState) + Base.kMatchMinLen;
                                    state.UpdateRep();
                                }
                                else
                                {
                                    rep3 = rep2;
                                    rep2 = rep1;
                                    rep1 = rep0;
                                    len = Base.kMatchMinLen + m_LenDecoder.Decode(m_RangeDecoder, posState);
                                    state.UpdateMatch();
                                    uint posSlot = m_PosSlotDecoder[Base.GetLenToPosState((int)len)].Decode(m_RangeDecoder);
                                    if (posSlot >= Base.kStartPosModelIndex)
                                    {
                                        int numDirectBits = (int)((posSlot >> 1) - 1);
                                        rep0 = (2 | (posSlot & 1)) << numDirectBits;
                                        if (posSlot < Base.kEndPosModelIndex)
                                            rep0 += BitTreeDecoder.ReverseDecode(m_PosDecoders,
                                                rep0 - posSlot - 1, m_RangeDecoder, numDirectBits);
                                        else
                                        {
                                            rep0 += m_RangeDecoder.DecodeDirectBits(
                                                numDirectBits - Base.kNumAlignBits) << Base.kNumAlignBits;
                                            rep0 += m_PosAlignDecoder.ReverseDecode(m_RangeDecoder);
                                        }
                                    }
                                    else
                                        rep0 = posSlot;
                                }

                                if (rep0 >= m_OutWindow.TrainSize + nowPos64 || rep0 >= m_DictionarySizeCheck)
                                {
                                    if (rep0 == 0xFFFFFFFF)
                                        break;
                                    throw new DataErrorException();
                                }

                                token.ThrowIfCancellationRequested();
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    m_OutWindow.CopyBlock(rep0, len);
                                else
                                    await m_OutWindow.CopyBlockAsync(rep0, len, token).ConfigureAwait(false);
                                nowPos64 += len;
                            }
                        }
                        if (blnSync)
                        {
                            progress?.SetProgress((long)outSize64, (long)nowPos64);
                        }
                        else if (progressAsync != null)
                        {
                            await progressAsync.SetProgressAsync((long)outSize64, (long)nowPos64, token).ConfigureAwait(false);
                        }
                    }
                }
            }
            finally
            {
                await m_OutWindow.FlushAsync(token).ConfigureAwait(false);
                await m_OutWindow.ReleaseStreamAsync(token).ConfigureAwait(false);
                m_RangeDecoder.ReleaseStream();
            }
        }

        public void SetDecoderProperties(byte[] properties)
        {
            if (properties.Length < 5)
                throw new InvalidParamException();
            int remainder = MathExtensions.DivRem(properties[0], 9, out int lc);
            int pb = remainder.DivRem(5, out int lp);
            if (pb > Base.kNumPosStatesBitsMax)
                throw new InvalidParamException();
            uint dictionarySize = BitConverter.ToUInt32(properties, 1);
            SetDictionarySize(dictionarySize);
            SetLiteralProperties(lp, lc);
            SetPosBitsProperties(pb);
        }

        public bool Train(Stream stream)
        {
            _solid = true;
            return m_OutWindow.Train(stream);
        }

        public Task<bool> TrainAsync(Stream stream, CancellationToken token = default)
        {
            _solid = true;
            return m_OutWindow.TrainAsync(stream, token);
        }

        /*
		public override bool CanRead { get { return true; }}
		public override bool CanWrite { get { return true; }}
		public override bool CanSeek { get { return true; }}
		public override long Length { get { return 0; }}
		public override long Position
		{
			get { return 0;	}
			set { }
		}
		public override void Flush() { }
		public override int Read(byte[] buffer, int offset, int count)
		{
			return 0;
		}
		public override void Write(byte[] buffer, int offset, int count)
		{
		}
		public override long Seek(long offset, System.IO.SeekOrigin origin)
		{
			return 0;
		}
		public override void SetLength(long value) {}
		*/
    }
}
