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
using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Chummer
{
    public static class BitArrayExtensions
    {
        /// <summary>
        /// Get the first element in a BitArray that matches <paramref name="blnValue"/>.
        /// </summary>
        /// <param name="ablnArray">Array to search.</param>
        /// <param name="blnValue">Value for which to look.</param>
        /// <param name="intFrom">Index from which to start search (inclusive).</param>
        /// <param name="intTo">Index at which to end search (exclusive).</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FirstMatching(this BitArray ablnArray, bool blnValue, int intFrom = 0, int intTo = int.MaxValue)
        {
            if (ablnArray == null)
                throw new ArgumentNullException(nameof(ablnArray));
            if (intTo > ablnArray.Count)
                intTo = ablnArray.Count;
            for (; intFrom < intTo; ++intFrom)
            {
                if (ablnArray[intFrom] == blnValue)
                    return intFrom;
            }
            return -1;
        }

        /// <summary>
        /// Count the number of bits set in a BitArray using special bit twiddling.
        /// Adapted from https://stackoverflow.com/questions/5063178/counting-bits-set-in-a-net-bitarray-class
        /// and http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel.
        /// </summary>
        /// <param name="ablnToCount"></param>
        /// <returns></returns>
        public static int CountTrues(this BitArray ablnToCount)
        {
            // Can't use stackalloc because BitArray doesn't have a CopyTo implementation that works with span
            int[] aintToCountMask = new int[(ablnToCount.Count >> 5) + 1];
            ablnToCount.CopyTo(aintToCountMask, 0);
            // Fix for not truncated bits in last integer that may have been set to true with SetAll()
            aintToCountMask[aintToCountMask.Length - 1] &= ~(-1 << (ablnToCount.Count % 32));
            int intReturn = 0;
            for (int i = 0; i < aintToCountMask.Length; i++)
            {
                int intLoopBlock = aintToCountMask[i];
                // magic (http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel)
                unchecked
                {
                    intLoopBlock -= ((intLoopBlock >> 1) & 0x55555555);
                    intLoopBlock = (intLoopBlock & 0x33333333) + ((intLoopBlock >> 2) & 0x33333333);
                    intLoopBlock = ((intLoopBlock + (intLoopBlock >> 4) & 0xF0F0F0F) * 0x1010101) >> 24;
                }
                intReturn += intLoopBlock;
            }
            return intReturn;
        }
    }
}
