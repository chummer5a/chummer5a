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
// LzmaBase.cs

namespace SevenZip.Compression.LZMA
{
    internal abstract class Base
    {
        public const int kNumRepDistances = 4;
        public const int kNumStates = 12;

        // static byte []kLiteralNextStates  = {0, 0, 0, 0, 1, 2, 3, 4,  5,  6,   4, 5};
        // static byte []kMatchNextStates    = {7, 7, 7, 7, 7, 7, 7, 10, 10, 10, 10, 10};
        // static byte []kRepNextStates      = {8, 8, 8, 8, 8, 8, 8, 11, 11, 11, 11, 11};
        // static byte []kShortRepNextStates = {9, 9, 9, 9, 9, 9, 9, 11, 11, 11, 11, 11};

        public struct State
        {
            public int Index;

            public void Init()
            { Index = 0; }

            public void UpdateChar()
            {
                if (Index < 4) Index = 0;
                else if (Index < 10) Index -= 3;
                else Index -= 6;
            }

            public void UpdateMatch()
            { Index = Index < 7 ? 7 : 10; }

            public void UpdateRep()
            { Index = Index < 7 ? 8 : 11; }

            public void UpdateShortRep()
            { Index = Index < 7 ? 9 : 11; }

            public bool IsCharState()
            { return Index < 7; }
        }

        public const int kNumPosSlotBits = 6;
        public const int kDicLogSizeMin = 0;
        // public const int kDicLogSizeMax = 30;
        // public const uint kDistTableSizeMax = kDicLogSizeMax * 2;

        public const int kNumLenToPosStatesBits = 2; // it's for speed optimization
        public const int kNumLenToPosStates = 1 << kNumLenToPosStatesBits;

        public const int kMatchMinLen = 2;

        public static int GetLenToPosState(int len)
        {
            len -= kMatchMinLen;
            if (len < kNumLenToPosStates)
                return len;
            return kNumLenToPosStates - 1;
        }

        public const int kNumAlignBits = 4;
        public const int kAlignTableSize = 1 << kNumAlignBits;
        public const int kAlignMask = kAlignTableSize - 1;

        public const int kStartPosModelIndex = 4;
        public const int kEndPosModelIndex = 14;
        public const int kNumPosModels = kEndPosModelIndex - kStartPosModelIndex;

        public const int kNumFullDistances = 1 << (kEndPosModelIndex / 2);

        public const int kNumLitPosStatesBitsEncodingMax = 4;
        public const int kNumLitContextBitsMax = 8;

        public const int kNumPosStatesBitsMax = 4;
        public const int kNumPosStatesMax = 1 << kNumPosStatesBitsMax;
        public const int kNumPosStatesBitsEncodingMax = 4;
        public const int kNumPosStatesEncodingMax = 1 << kNumPosStatesBitsEncodingMax;

        public const int kNumLowLenBits = 3;
        public const int kNumMidLenBits = 3;
        public const int kNumHighLenBits = 8;
        public const int kNumLowLenSymbols = 1 << kNumLowLenBits;
        public const int kNumMidLenSymbols = 1 << kNumMidLenBits;

        public const int kNumLenSymbols = kNumLowLenSymbols + kNumMidLenSymbols +
                (1 << kNumHighLenBits);

        public const int kMatchMaxLen = kMatchMinLen + kNumLenSymbols - 1;
    }
}
