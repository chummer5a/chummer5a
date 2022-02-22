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
            return (intA + Math.Sign(intA) * (Math.Abs(intB) - 1)) / intB; // Adding 1 if modulo > 0 would require a separate modulo operation that is as slow as division
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
                case 2: // Extremely common case, so handle it explicitly
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
                    return 1 - 2 * (Math.Abs(intPower) & 1);
            }
            // Integer division always rounds towards zero, so every base except the ones already handled ends up producing 0 after rounding
            if (intPower < 0)
                return 0;
            int intReturn = 1;
            int i;
            // Dual loop structure looks funky, but cuts down on number of multiplication operations in worst case scenarios compared to a single loop
            for (; intPower > 1; intPower -= i / 2)
            {
                int intLoopElement = intBase;
                for (i = 2; i <= intPower; i *= 2)
                {
                    intLoopElement *= intLoopElement;
                }
                intReturn *= intLoopElement;
            }
            return intReturn;
        }
    }
}
