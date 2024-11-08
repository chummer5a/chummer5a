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
using System.Runtime.CompilerServices;

namespace Chummer
{
    public static class IntegerExtensions
    {
        /// <summary>
        /// Syntactic sugar for doing integer division that always rounds away from zero instead of towards zero.
        /// </summary>
        /// <param name="intA">Dividend integer.</param>
        /// <param name="intB">Divisor integer.</param>
        /// <returns><paramref name="intA"/> divided by <paramref name="intB"/>, rounded towards the nearest number away from zero (up if positive, down if negative).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int DivAwayFromZero(this int intA, int intB)
        {
            if (intB == 0)
                throw new DivideByZeroException();
            if (intA == 0)
                return 0;
            // Adding 1 if modulo > 0 would require a separate modulo operation that is as slow as division
            int intParityA = intA > 0 ? 1 : -1;
            int intParityB = intB > 0 ? 1 : -1;
            return (intA - intParityA + intParityA * intParityB * intB) / intB;
        }

        /// <summary>
        /// Exponentiates an integer by another integer, always staying within the realm of integers.
        /// </summary>
        /// <param name="intBase">Number to exponentiate.</param>
        /// <param name="intPower">Power to which to raise <paramref name="intBase"/>.</param>
        /// <returns><paramref name="intBase"/> to the power of <paramref name="intPower"/>.</returns>
        internal static int RaiseToPower(this int intBase, int intPower)
        {
            switch (intPower)
            {
                case 3: // (Potentially) common case, handle explicitly
                    if (intBase >= 1291 || intBase <= -1291) // cubing this will cause an overflow exception, so break
                    {
                        Utils.BreakIfDebug();
                        throw new ArgumentException("Base is too big to be cubed and still stay an integer.", nameof(intBase));
                    }

                    return intBase * intBase * intBase;

                case 2: // Extremely common case, so handle it explicitly
                    if (intBase >= 46341 || intBase <= -46341) // squaring this will cause an overflow exception, so break
                    {
                        Utils.BreakIfDebug();
                        throw new ArgumentException("Base is too big to be squared and still stay an integer.", nameof(intBase));
                    }

                    return intBase * intBase;

                case 1:
                    return intBase;

                case 0: // Yes, even 0^0 should return 1 per IEEE specifications
                    return 1;
            }
            switch (intBase)
            {
                case 1:
                    return 1;

                case 0:
                    if (intPower < 0)
                        throw new DivideByZeroException();
                    return 0;

                case -1:
                    return (Math.Abs(intPower) & 1) == 0 ? 1 : -1;
            }
            // Integer division always rounds towards zero, so every base except the ones already handled ends up producing 0 after rounding
            if (intPower < 0)
                return 0;
            // Special case when both the base and the exponent are powers of 2, since we can make things faster by bit shifting
            if ((intBase & (intBase - 1)) == 0 && (intPower & (intPower - 1)) == 0)
            {
                for (; intPower > 1; intPower >>= 1)
                {
                    intBase <<= 1;
                }

                return intBase;
            }
            int intReturn = 1;
            int i;
            // Dual loop structure looks funky, but cuts down on number of multiplication operations in worst case scenarios compared to a single loop
            for (; intPower > 1; intPower -= i >> 1)
            {
                int intLoopElement = intBase;
                for (i = 2; i <= intPower; i <<= 1)
                {
                    intLoopElement *= intLoopElement;
                }
                intReturn *= intLoopElement;
            }
            return intReturn;
        }

        /// <summary>
        /// Exponentiates an integer by another integer, returning a 64-bit integer.
        /// </summary>
        /// <param name="intBase">Number to exponentiate.</param>
        /// <param name="intPower">Power to which to raise <paramref name="intBase"/>.</param>
        /// <returns><paramref name="intBase"/> to the power of <paramref name="intPower"/>.</returns>
        internal static long RaiseToPowerSafe(this int intBase, int intPower)
        {
            switch (intPower)
            {
                case 3:
                {
                    long lngBase = intBase;
                    if (lngBase > 2097152 || intBase < -2097152) // cubing this will cause an overflow exception, so break
                    {
                        Utils.BreakIfDebug();
                        throw new ArgumentException("Base is too big to be cubed and still stay a 64-bit integer.", nameof(intBase));
                    }

                    return lngBase * lngBase * lngBase;
                }

                case 2:
                {
                    long lngBase = intBase;
                    return lngBase * lngBase;
                }

                case 1:
                    return intBase;

                case 0: // Yes, even 0^0 should return 1 per IEEE specifications
                    return 1;
            }
            switch (intBase)
            {
                case 1:
                    return 1;

                case 0:
                    if (intPower < 0)
                        throw new DivideByZeroException();
                    return 0;

                case -1:
                    return (Math.Abs(intPower) & 1) == 0 ? 1 : -1;
            }
            // Integer division always rounds towards zero, so every base except the ones already handled ends up producing 0 after rounding
            if (intPower < 0)
                return 0;
            // Special case when both the base and the exponent are powers of 2, since we can make things faster by bit shifting
            if ((intBase & (intBase - 1)) == 0 && (intPower & (intPower - 1)) == 0)
            {
                long lngBase = intBase;
                for (; intPower > 1; intPower >>= 1)
                {
                    lngBase <<= 1;
                }

                return lngBase;
            }
            long lngReturn = 1;
            int i;
            // Dual loop structure looks funky, but cuts down on number of multiplication operations in worst case scenarios compared to a single loop
            for (; intPower > 1; intPower -= i >> 1)
            {
                long lngLoopElement = intBase;
                for (i = 2; i <= intPower; i <<= 1)
                {
                    lngLoopElement *= lngLoopElement;
                }
                lngReturn *= lngLoopElement;
            }
            return lngReturn;
        }

        /// <summary>
        /// Exponentiates a short by another short, returning a 64-bit integer.
        /// </summary>
        /// <param name="shtBase">Number to exponentiate.</param>
        /// <param name="shtPower">Power to which to raise <paramref name="shtBase"/>.</param>
        /// <returns><paramref name="shtBase"/> to the power of <paramref name="shtPower"/>.</returns>
        internal static long RaiseToPowerSafe(this short shtBase, short shtPower)
        {
            switch (shtPower)
            {
                case 3: // (Potentially) common case, handle explicitly
                {
                    long lngBase = shtBase;
                    return lngBase * lngBase * lngBase;
                }

                case 2: // Extremely common case, so handle it explicitly
                {
                    long lngBase = shtBase;
                    return lngBase * lngBase;
                }

                case 1:
                    return shtBase;

                case 0: // Yes, even 0^0 should return 1 per IEEE specifications
                    return 1;
            }
            switch (shtBase)
            {
                case 1:
                    return 1;

                case 0:
                    if (shtPower < 0)
                        throw new DivideByZeroException();
                    return 0;

                case -1:
                    return (Math.Abs(shtPower) & 1) == 0 ? 1 : -1;
            }
            // Integer division always rounds towards zero, so every base except the ones already handled ends up producing 0 after rounding
            if (shtPower < 0)
                return 0;
            // Special case when both the base and the exponent are powers of 2, since we can make things faster by bit shifting
            if ((shtBase & (shtBase - 1)) == 0 && (shtPower & (shtPower - 1)) == 0)
            {
                long lngBase = shtBase;
                for (; shtPower > 1; shtPower >>= 1)
                {
                    lngBase <<= 1;
                }

                return lngBase;
            }
            long lngReturn = 1;
            short i;
            // Dual loop structure looks funky, but cuts down on number of multiplication operations in worst case scenarios compared to a single loop
            for (; shtPower > 1; shtPower -= (short)(i >> 1))
            {
                long lngLoopElement = shtBase;
                for (i = 2; i <= shtPower; i <<= 1)
                {
                    lngLoopElement *= lngLoopElement;
                }
                lngReturn *= lngLoopElement;
            }
            return lngReturn;
        }

        /// <summary>
        /// Exponentiates a signed sbye by another signed sbye, returning a 64-bit integer.
        /// </summary>
        /// <param name="sbyBase">Number to exponentiate.</param>
        /// <param name="sbyPower">Power to which to raise <paramref name="sbyBase"/>.</param>
        /// <returns><paramref name="sbyBase"/> to the power of <paramref name="sbyPower"/>.</returns>
        internal static long RaiseToPowerSafe(this sbyte sbyBase, sbyte sbyPower)
        {
            switch (sbyPower)
            {
                case 3: // (Potentially) common case, handle explicitly
                    {
                        long lngBase = sbyBase;
                        return lngBase * lngBase * lngBase;
                    }

                case 2: // Extremely common case, so handle it explicitly
                    {
                        long lngBase = sbyBase;
                        return lngBase * lngBase;
                    }

                case 1:
                    return sbyBase;

                case 0: // Yes, even 0^0 should return 1 per IEEE specifications
                    return 1;
            }
            switch (sbyBase)
            {
                case 1:
                    return 1;

                case 0:
                    if (sbyPower < 0)
                        throw new DivideByZeroException();
                    return 0;

                case -1:
                    return (Math.Abs(sbyPower) & 1) == 0 ? 1 : -1;
            }
            // Integer division always rounds towards zero, so every base except the ones already handled ends up producing 0 after rounding
            if (sbyPower < 0)
                return 0;
            // Special case when both the base and the exponent are powers of 2, since we can make things faster by bit shifting
            if ((sbyBase & (sbyBase - 1)) == 0 && (sbyPower & (sbyPower - 1)) == 0)
            {
                long lngBase = sbyBase;
                for (; sbyPower > 1; sbyPower >>= 1)
                {
                    lngBase <<= 1;
                }

                return lngBase;
            }
            long lngReturn = 1;
            sbyte i;
            // Dual loop structure looks funky, but cuts down on number of multiplication operations in worst case scenarios compared to a single loop
            for (; sbyPower > 1; sbyPower -= (sbyte)(i >> 1))
            {
                long lngLoopElement = sbyBase;
                for (i = 2; i <= sbyPower; i <<= 1)
                {
                    lngLoopElement *= lngLoopElement;
                }
                lngReturn *= lngLoopElement;
            }
            return lngReturn;
        }
    }
}
