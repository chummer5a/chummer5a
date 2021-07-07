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
using System.Buffers;
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
        /// <param name="ablnToCount">Array to process</param>
        /// <returns></returns>
        public static int CountTrues(this BitArray ablnToCount)
        {
            if (ablnToCount == null)
                throw new ArgumentNullException(nameof(ablnToCount));
            int intMaskSize = ablnToCount.Count >> 5;
            int intArraySizeModulo32 = ablnToCount.Count % 32;
            if (intArraySizeModulo32 != 0)
                intMaskSize += 1;
            // Can't use stackalloc because BitArray doesn't have a CopyTo implementation that works with span
            int[] aintToCountMask = intMaskSize > GlobalOptions.MaxStackLimit ? ArrayPool<int>.Shared.Rent(intMaskSize) :  new int[intMaskSize];
            ablnToCount.CopyTo(aintToCountMask, 0);
            // Fix for not truncated bits in last integer that may have been set to true with SetAll()
            aintToCountMask[aintToCountMask.Length - 1] &= ~(-1 << intArraySizeModulo32);
            int intReturn = 0;
            foreach (int intLoop in aintToCountMask)
            {
                int intLoopBlock = intLoop;
                // magic (http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel)
                unchecked
                {
                    intLoopBlock -= ((intLoopBlock >> 1) & 0x55555555);
                    intLoopBlock = (intLoopBlock & 0x33333333) + ((intLoopBlock >> 2) & 0x33333333);
                    intLoopBlock = ((intLoopBlock + (intLoopBlock >> 4) & 0xF0F0F0F) * 0x1010101) >> 24;
                }
                intReturn += intLoopBlock;
            }
            if (intMaskSize > GlobalOptions.MaxStackLimit)
                ArrayPool<int>.Shared.Return(aintToCountMask);
            return intReturn;
        }

        /// <summary>
        /// Check if all bits in a bit array are set or unset.
        /// </summary>
        /// <param name="ablnArray">Array to process</param>
        /// <param name="blnValue">True if we are checking set bits, false if we are checking unset bits.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AllValue(this BitArray ablnArray, bool blnValue)
        {
            // Interestingly enough, the lazy method of just iterating through all bits in the BitArray is faster in all of our use cases than casting
            // the array into an int32 array and then doing fancy bit twiddling with it.
            return ablnArray.FirstMatching(!blnValue) < 0;
        }
    }
}
