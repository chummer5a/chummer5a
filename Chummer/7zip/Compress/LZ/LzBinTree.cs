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
// LzBinTree.cs

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SevenZip.Compression.LZ
{
    [CLSCompliant(false)]
    public class BinTree : InWindow, IMatchFinder
    {
        private int _cyclicBufferPos;
        private int _cyclicBufferSize;
        private int _matchMaxLen;

        private uint[] _son;
        private uint[] _hash;

        private uint _cutValue = 0xFF;
        private uint _hashMask;
        private uint _hashSizeSum;

        private bool HASH_ARRAY = true;

        private const uint kHash2Size = 1 << 10;
        private const uint kHash3Size = 1 << 16;
        private const uint kBT2HashSize = 1 << 16;
        private const int kStartMaxLen = 1;
        private const uint kHash3Offset = kHash2Size;
        private const uint kEmptyHashValue = 0;
        private const int kMaxValForNormalize = 134217728 + 256; // Set to make sure the uint arrays created are within the maximum array size of 32-bit .NET Framework (2^30 bytes max -> ceil((2^30 - 1)/2/sizeof(uint)) = this value)

        private int kNumHashDirectBytes;
        private uint kMinMatchCheck = 4;
        private uint kFixHashSize = kHash2Size + kHash3Size;

        public void SetType(int numHashBytes)
        {
            HASH_ARRAY = numHashBytes > 2;
            if (HASH_ARRAY)
            {
                kNumHashDirectBytes = 0;
                kMinMatchCheck = 4;
                kFixHashSize = kHash2Size + kHash3Size;
            }
            else
            {
                kNumHashDirectBytes = 2;
                kMinMatchCheck = 2 + 1;
                kFixHashSize = 0;
            }
        }

        public override void Init()
        {
            base.Init();
            for (uint i = 0; i < _hashSizeSum; i++)
                _hash[i] = kEmptyHashValue;
            _cyclicBufferPos = 0;
            ReduceOffsets(-1);
        }

        public override async Task InitAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await base.InitAsync(token).ConfigureAwait(false);
            for (uint i = 0; i < _hashSizeSum; i++)
                _hash[i] = kEmptyHashValue;
            _cyclicBufferPos = 0;
            ReduceOffsets(-1);
        }

        public override void MovePos()
        {
            if (++_cyclicBufferPos >= _cyclicBufferSize)
                _cyclicBufferPos = 0;
            base.MovePos();
            if (_pos == kMaxValForNormalize)
                Normalize();
        }

        public override async Task MovePosAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (++_cyclicBufferPos >= _cyclicBufferSize)
                _cyclicBufferPos = 0;
            await base.MovePosAsync(token).ConfigureAwait(false);
            if (_pos == kMaxValForNormalize)
                Normalize();
        }

        [CLSCompliant(false)]
        public override int GetMatchLen(int index, int distance, int limit)
        {
            unchecked
            {
                if (_streamEndWasReached && _pos + index + limit > _streamPos)
                    limit = _streamPos - (_pos + index);
                distance++;
                // Byte *pby = _buffer + (size_t)_pos + index;
                int pby = _bufferOffset + _pos + index;
                int pby2 = pby - distance;
                return GetMatchLengthFast(limit, pby, pby2);
            }
        }

        [CLSCompliant(false)]
        public void Create(int historySize, int keepAddBufferBefore,
                int matchMaxLen, int keepAddBufferAfter)
        {
            if (historySize > kMaxValForNormalize - 256)
                throw new ArgumentOutOfRangeException(nameof(historySize));
            unchecked
            {
                _cutValue = 16 + ((uint)matchMaxLen >> 1);

                int windowReserveSize = (historySize + keepAddBufferBefore +
                                          matchMaxLen + keepAddBufferAfter) / 2 + 256;

                base.Create(historySize + keepAddBufferBefore, matchMaxLen + keepAddBufferAfter, windowReserveSize);

                _matchMaxLen = matchMaxLen;

                int cyclicBufferSize = historySize + 1;
                if (_cyclicBufferSize != cyclicBufferSize)
                    _son = new uint[(_cyclicBufferSize = cyclicBufferSize) * 2];

                uint hs = kBT2HashSize;

                if (HASH_ARRAY)
                {
                    hs = (uint)historySize - 1;
                    hs |= hs >> 1;
                    hs |= hs >> 2;
                    hs |= hs >> 4;
                    hs |= hs >> 8;
                    hs >>= 1;
                    hs |= 0xFFFF;
                    if (hs > 1 << 24)
                        hs >>= 1;
                    _hashMask = hs;
                    hs++;
                    hs += kFixHashSize;
                }

                if (hs != _hashSizeSum)
                    _hash = new uint[_hashSizeSum = hs];
            }
        }

        // Faster version of bottleneck code, inspired by the following post: https://stackoverflow.com/a/17598461
        private unsafe int GetMatchLengthFast(int lenLimit, int cur, int pby1, int len = 0, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            const int size = sizeof(ulong);
            fixed (byte* p1 = &_bufferBase[cur])
            fixed (byte* p2 = &_bufferBase[pby1])
            {
                // First do equality comparisons 8 bytes at a time to speed things up
                if (lenLimit >= size)
                {
                    int longLenLimit = lenLimit - size + 1;
                    while (len < longLenLimit && *(ulong*)(p1 + len) == *(ulong*)(p2 + len))
                    {
                        token.ThrowIfCancellationRequested();
                        len += size;
                    }
                }
                while (len < lenLimit && *(p1 + len) == *(p2 + len))
                {
                    token.ThrowIfCancellationRequested();
                    ++len;
                }
            }
            return len;
        }

        [CLSCompliant(false)]
        public int GetMatches(int[] distances)
        {
            unchecked
            {
                int lenLimit;
                if (_pos + _matchMaxLen <= _streamPos)
                    lenLimit = _matchMaxLen;
                else
                {
                    lenLimit = _streamPos - _pos;
                    if (lenLimit < kMinMatchCheck)
                    {
                        MovePos();
                        return 0;
                    }
                }

                int offset = 0;
                int matchMinPos = _pos > _cyclicBufferSize ? _pos - _cyclicBufferSize : 0;
                int cur = _bufferOffset + _pos;
                int maxLen = kStartMaxLen; // to avoid items for len < hashSize;
                uint hashValue;
                uint curMatch;

                if (HASH_ARRAY)
                {
                    byte curValue = _bufferBase[cur];
                    uint temp = CRC.Table[curValue] ^ _bufferBase[cur + 1];
                    uint hash2Value = temp & (kHash2Size - 1);
                    temp ^= (uint)_bufferBase[cur + 2] << 8;
                    uint hash3Value = temp & (kHash3Size - 1);
                    hashValue = (temp ^ (CRC.Table[_bufferBase[cur + 3]] << 5)) & _hashMask;
                    curMatch = _hash[kFixHashSize + hashValue];
                    uint curMatch2 = _hash[hash2Value];
                    uint curMatch3 = _hash[kHash3Offset + hash3Value];
                    _hash[hash2Value] = (uint)_pos;
                    _hash[kHash3Offset + hash3Value] = (uint)_pos;
                    if (curMatch2 > matchMinPos && _bufferBase[_bufferOffset + curMatch2] == curValue)
                    {
                        distances[offset++] = maxLen = 2;
                        distances[offset++] = (int)((uint)_pos - curMatch2 - 1);
                    }

                    if (curMatch3 > matchMinPos && _bufferBase[_bufferOffset + curMatch3] == curValue)
                    {
                        if (curMatch3 == curMatch2)
                            offset -= 2;
                        distances[offset++] = maxLen = 3;
                        distances[offset++] = (int)((uint)_pos - curMatch3 - 1);
                        curMatch2 = curMatch3;
                    }

                    if (offset != 0 && curMatch2 == curMatch)
                    {
                        offset -= 2;
                        maxLen = kStartMaxLen;
                    }
                }
                else
                {
                    hashValue = _bufferBase[cur] ^ ((uint)_bufferBase[cur + 1] << 8);
                    curMatch = _hash[kFixHashSize + hashValue];
                }

                _hash[kFixHashSize + hashValue] = (uint)_pos;

                int ptr0 = (_cyclicBufferPos << 1) + 1;
                int ptr1 = _cyclicBufferPos << 1;

                int len1;
                int len0 = len1 = kNumHashDirectBytes;

                if (kNumHashDirectBytes != 0 && curMatch > matchMinPos
                                             && _bufferBase[_bufferOffset + curMatch + kNumHashDirectBytes] !=
                                             _bufferBase[cur + kNumHashDirectBytes])
                {
                    distances[offset++] = maxLen = kNumHashDirectBytes;
                    distances[offset++] = (int)((uint)_pos - curMatch - 1);
                }

                uint count = _cutValue;

                while (true)
                {
                    if (curMatch <= matchMinPos || count-- == 0)
                    {
                        _son[ptr0] = _son[ptr1] = kEmptyHashValue;
                        break;
                    }

                    int delta = _pos - (int)curMatch;
                    int cyclicPos = (delta <= _cyclicBufferPos
                        ? _cyclicBufferPos - delta
                        : _cyclicBufferPos - delta + _cyclicBufferSize) << 1;

                    int pby1 = _bufferOffset + (int)curMatch;
                    int len = Math.Min(len0, len1);
                    byte left = _bufferBase[pby1 + len];
                    byte right = _bufferBase[cur + len];
                    if (left == right)
                    {
                        len = GetMatchLengthFast(lenLimit, cur, pby1, len);

                        if (maxLen < len)
                        {
                            distances[offset++] = maxLen = len;
                            distances[offset++] = delta - 1;
                            if (len == lenLimit)
                            {
                                _son[ptr1] = _son[cyclicPos];
                                _son[ptr0] = _son[cyclicPos + 1];
                                break;
                            }
                        }

                        left = _bufferBase[pby1 + len];
                        right = _bufferBase[cur + len];
                    }

                    if (left < right)
                    {
                        _son[ptr1] = curMatch;
                        ptr1 = cyclicPos + 1;
                        curMatch = _son[ptr1];
                        len1 = len;
                    }
                    else
                    {
                        _son[ptr0] = curMatch;
                        ptr0 = cyclicPos;
                        curMatch = _son[ptr0];
                        len0 = len;
                    }
                }

                MovePos();
                return offset;
            }
        }

        [CLSCompliant(false)]
        public async Task<int> GetMatchesAsync(int[] distances, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            unchecked
            {
                int lenLimit;
                if (_pos + _matchMaxLen <= _streamPos)
                    lenLimit = _matchMaxLen;
                else
                {
                    lenLimit = _streamPos - _pos;
                    if (lenLimit < kMinMatchCheck)
                    {
                        await MovePosAsync(token).ConfigureAwait(false);
                        return 0;
                    }
                }

                int offset = 0;
                int matchMinPos = _pos > _cyclicBufferSize ? _pos - _cyclicBufferSize : 0;
                int cur = _bufferOffset + _pos;
                int maxLen = kStartMaxLen; // to avoid items for len < hashSize;
                uint hashValue;
                uint curMatch;

                if (HASH_ARRAY)
                {
                    byte curValue = _bufferBase[cur];
                    uint temp = CRC.Table[curValue] ^ _bufferBase[cur + 1];
                    uint hash2Value = temp & (kHash2Size - 1);
                    temp ^= (uint)_bufferBase[cur + 2] << 8;
                    uint hash3Value = temp & (kHash3Size - 1);
                    hashValue = (temp ^ (CRC.Table[_bufferBase[cur + 3]] << 5)) & _hashMask;
                    curMatch = _hash[kFixHashSize + hashValue];
                    uint curMatch2 = _hash[hash2Value];
                    uint curMatch3 = _hash[kHash3Offset + hash3Value];
                    _hash[hash2Value] = (uint)_pos;
                    _hash[kHash3Offset + hash3Value] = (uint)_pos;
                    if (curMatch2 > matchMinPos && _bufferBase[_bufferOffset + curMatch2] == curValue)
                    {
                        distances[offset++] = maxLen = 2;
                        distances[offset++] = (int)((uint)_pos - curMatch2 - 1);
                    }

                    if (curMatch3 > matchMinPos && _bufferBase[_bufferOffset + curMatch3] == curValue)
                    {
                        if (curMatch3 == curMatch2)
                            offset -= 2;
                        distances[offset++] = maxLen = 3;
                        distances[offset++] = (int)((uint)_pos - curMatch3 - 1);
                        curMatch2 = curMatch3;
                    }

                    if (offset != 0 && curMatch2 == curMatch)
                    {
                        offset -= 2;
                        maxLen = kStartMaxLen;
                    }
                }
                else
                {
                    hashValue = _bufferBase[cur] ^ ((uint)_bufferBase[cur + 1] << 8);
                    curMatch = _hash[kFixHashSize + hashValue];
                }

                _hash[kFixHashSize + hashValue] = (uint)_pos;

                int ptr0 = (_cyclicBufferPos << 1) + 1;
                int ptr1 = _cyclicBufferPos << 1;

                int len1;
                int len0 = len1 = kNumHashDirectBytes;

                if (kNumHashDirectBytes != 0 && curMatch > matchMinPos
                                             && _bufferBase[_bufferOffset + curMatch + kNumHashDirectBytes] !=
                                             _bufferBase[cur + kNumHashDirectBytes])
                {
                    distances[offset++] = maxLen = kNumHashDirectBytes;
                    distances[offset++] = (int)((uint)_pos - curMatch - 1);
                }

                uint count = _cutValue;

                while (true)
                {
                    if (curMatch <= matchMinPos || count-- == 0)
                    {
                        _son[ptr0] = _son[ptr1] = kEmptyHashValue;
                        break;
                    }

                    int delta = _pos - (int)curMatch;
                    int cyclicPos = (delta <= _cyclicBufferPos
                        ? _cyclicBufferPos - delta
                        : _cyclicBufferPos - delta + _cyclicBufferSize) << 1;

                    int pby1 = _bufferOffset + (int)curMatch;
                    int len = Math.Min(len0, len1);
                    byte left = _bufferBase[pby1 + len];
                    byte right = _bufferBase[cur + len];
                    if (left == right)
                    {
                        len = GetMatchLengthFast(lenLimit, cur, pby1, len);

                        if (maxLen < len)
                        {
                            distances[offset++] = maxLen = len;
                            distances[offset++] = delta - 1;
                            if (len == lenLimit)
                            {
                                _son[ptr1] = _son[cyclicPos];
                                _son[ptr0] = _son[cyclicPos + 1];
                                break;
                            }
                        }

                        left = _bufferBase[pby1 + len];
                        right = _bufferBase[cur + len];
                    }

                    if (left < right)
                    {
                        _son[ptr1] = curMatch;
                        ptr1 = cyclicPos + 1;
                        curMatch = _son[ptr1];
                        len1 = len;
                    }
                    else
                    {
                        _son[ptr0] = curMatch;
                        ptr0 = cyclicPos;
                        curMatch = _son[ptr0];
                        len0 = len;
                    }
                }

                await MovePosAsync(token).ConfigureAwait(false);
                return offset;
            }
        }

        [CLSCompliant(false)]
        public void Skip(int num)
        {
            unchecked
            {
                do
                {
                    int lenLimit;
                    if (_pos + _matchMaxLen <= _streamPos)
                        lenLimit = _matchMaxLen;
                    else
                    {
                        lenLimit = _streamPos - _pos;
                        if (lenLimit < kMinMatchCheck)
                        {
                            MovePos();
                            continue;
                        }
                    }

                    int matchMinPos = _pos > _cyclicBufferSize ? _pos - _cyclicBufferSize : 0;
                    int cur = _bufferOffset + _pos;

                    uint hashValue;

                    if (HASH_ARRAY)
                    {
                        uint temp = CRC.Table[_bufferBase[cur]] ^ _bufferBase[cur + 1];
                        uint hash2Value = temp & (kHash2Size - 1);
                        _hash[hash2Value] = (uint)_pos;
                        temp ^= (uint)_bufferBase[cur + 2] << 8;
                        uint hash3Value = temp & (kHash3Size - 1);
                        _hash[kHash3Offset + hash3Value] = (uint)_pos;
                        hashValue = (temp ^ (CRC.Table[_bufferBase[cur + 3]] << 5)) & _hashMask;
                    }
                    else
                        hashValue = _bufferBase[cur] ^ ((uint)_bufferBase[cur + 1] << 8);

                    uint curMatch = _hash[kFixHashSize + hashValue];
                    _hash[kFixHashSize + hashValue] = (uint)_pos;

                    int ptr0 = (_cyclicBufferPos << 1) + 1;
                    int ptr1 = _cyclicBufferPos << 1;

                    int len1;
                    int len0 = len1 = kNumHashDirectBytes;

                    uint count = _cutValue;
                    while (true)
                    {
                        if (curMatch <= matchMinPos || count-- == 0)
                        {
                            _son[ptr0] = _son[ptr1] = kEmptyHashValue;
                            break;
                        }

                        int delta = _pos - (int)curMatch;
                        int cyclicPos = (delta <= _cyclicBufferPos
                            ? _cyclicBufferPos - delta
                            : _cyclicBufferPos - delta + _cyclicBufferSize) << 1;

                        int pby1 = _bufferOffset + (int)curMatch;
                        int len = Math.Min(len0, len1);
                        byte left = _bufferBase[pby1 + len];
                        byte right = _bufferBase[cur + len];
                        if (left == right)
                        {
                            len = GetMatchLengthFast(lenLimit, cur, pby1, len);
                            if (len == lenLimit)
                            {
                                _son[ptr1] = _son[cyclicPos];
                                _son[ptr0] = _son[cyclicPos + 1];
                                break;
                            }

                            left = _bufferBase[pby1 + len];
                            right = _bufferBase[cur + len];
                        }

                        if (left < right)
                        {
                            _son[ptr1] = curMatch;
                            ptr1 = cyclicPos + 1;
                            curMatch = _son[ptr1];
                            len1 = len;
                        }
                        else
                        {
                            _son[ptr0] = curMatch;
                            ptr0 = cyclicPos;
                            curMatch = _son[ptr0];
                            len0 = len;
                        }
                    }

                    MovePos();
                } while (--num != 0);
            }
        }

        [CLSCompliant(false)]
        public async Task SkipAsync(int num, CancellationToken token = default)
        {
            unchecked
            {
                do
                {
                    token.ThrowIfCancellationRequested();
                    int lenLimit;
                    if (_pos + _matchMaxLen <= _streamPos)
                        lenLimit = _matchMaxLen;
                    else
                    {
                        lenLimit = _streamPos - _pos;
                        if (lenLimit < kMinMatchCheck)
                        {
                            await MovePosAsync(token).ConfigureAwait(false);
                            continue;
                        }
                    }

                    int matchMinPos = _pos > _cyclicBufferSize ? _pos - _cyclicBufferSize : 0;
                    int cur = _bufferOffset + _pos;

                    uint hashValue;

                    if (HASH_ARRAY)
                    {
                        uint temp = CRC.Table[_bufferBase[cur]] ^ _bufferBase[cur + 1];
                        uint hash2Value = temp & (kHash2Size - 1);
                        _hash[hash2Value] = (uint)_pos;
                        temp ^= (uint)_bufferBase[cur + 2] << 8;
                        uint hash3Value = temp & (kHash3Size - 1);
                        _hash[kHash3Offset + hash3Value] = (uint)_pos;
                        hashValue = (temp ^ (CRC.Table[_bufferBase[cur + 3]] << 5)) & _hashMask;
                    }
                    else
                        hashValue = _bufferBase[cur] ^ ((uint)_bufferBase[cur + 1] << 8);

                    uint curMatch = _hash[kFixHashSize + hashValue];
                    _hash[kFixHashSize + hashValue] = (uint)_pos;

                    int ptr0 = (_cyclicBufferPos << 1) + 1;
                    int ptr1 = _cyclicBufferPos << 1;

                    int len1;
                    int len0 = len1 = kNumHashDirectBytes;

                    uint count = _cutValue;
                    while (true)
                    {
                        if (curMatch <= matchMinPos || count-- == 0)
                        {
                            _son[ptr0] = _son[ptr1] = kEmptyHashValue;
                            break;
                        }

                        int delta = _pos - (int)curMatch;
                        int cyclicPos = (delta <= _cyclicBufferPos
                            ? _cyclicBufferPos - delta
                            : _cyclicBufferPos - delta + _cyclicBufferSize) << 1;

                        int pby1 = _bufferOffset + (int)curMatch;
                        int len = Math.Min(len0, len1);
                        byte left = _bufferBase[pby1 + len];
                        byte right = _bufferBase[cur + len];
                        if (left == right)
                        {
                            len = GetMatchLengthFast(lenLimit, cur, pby1, len);
                            if (len == lenLimit)
                            {
                                _son[ptr1] = _son[cyclicPos];
                                _son[ptr0] = _son[cyclicPos + 1];
                                break;
                            }

                            left = _bufferBase[pby1 + len];
                            right = _bufferBase[cur + len];
                        }

                        if (left < right)
                        {
                            _son[ptr1] = curMatch;
                            ptr1 = cyclicPos + 1;
                            curMatch = _son[ptr1];
                            len1 = len;
                        }
                        else
                        {
                            _son[ptr0] = curMatch;
                            ptr0 = cyclicPos;
                            curMatch = _son[ptr0];
                            len0 = len;
                        }
                    }

                    await MovePosAsync(token).ConfigureAwait(false);
                } while (--num != 0);
            }
        }

        private static void NormalizeLinks(uint[] items, int numItems, uint subValue)
        {
            unchecked
            {
                for (uint i = 0; i < numItems; i++)
                {
                    uint value = items[i];
                    if (value <= subValue)
                        value = kEmptyHashValue;
                    else
                        value -= subValue;
                    items[i] = value;
                }
            }
        }

        private void Normalize()
        {
            unchecked
            {
                int subValue = _pos - _cyclicBufferSize;
                NormalizeLinks(_son, _cyclicBufferSize * 2, (uint)subValue);
                NormalizeLinks(_hash, (int)_hashSizeSum, (uint)subValue);
                ReduceOffsets(subValue);
            }
        }

        [CLSCompliant(false)]
        public void SetCutValue(uint cutValue)
        { _cutValue = cutValue; }
    }
}
