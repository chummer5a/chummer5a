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
    public static class DecimalExtensions
    {
        /// <summary>
        /// Syntactic sugar for rounding a decimal away from zero and then converting it to an integer.
        /// </summary>
        /// <param name="decToRound">Decimal to round.</param>
        /// <returns>Rounded integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int StandardRound(this decimal decToRound)
        {
            return decimal.ToInt32(decToRound >= 0 ? decimal.Ceiling(decToRound) : decimal.Floor(decToRound));
        }

        /// <summary>
        /// Syntactic sugar for applying ToInt32 conversion to a decimal (rounds towards zero).
        /// </summary>
        /// <param name="decIn">Decimal to convert.</param>
        /// <returns>Rounded integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ToInt32(this decimal decIn)
        {
            return decimal.ToInt32(decIn);
        }

        /// <summary>
        /// Exponentiates a decimal by another decimal without casting to floating point (and therefore maintaining exact precision) unless absolutely necessary.
        /// Note: can be quite slow compared to just using Math.Pow and casting the result, so use with care.
        /// </summary>
        /// <param name="decBase">Number to exponentiate.</param>
        /// <param name="decPower">Power to which to raise <paramref name="decBase"/>.</param>
        /// <returns><paramref name="decBase"/> to the power of <paramref name="decPower"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static decimal RaiseToPower(this decimal decBase, decimal decPower)
        {
            int intPower = decPower.ToInt32();
            return intPower == decPower
                ? decBase.RaiseToPower(intPower)
                // Use Math.Pow for doing (square) roots and fractional exponents because we kind of have to, there's no easy way to do roots with built-in decimal arithmetic
                : Convert.ToDecimal(Math.Pow(Convert.ToDouble(decBase), Convert.ToDouble(decPower)));
        }

        /// <summary>
        /// Exponentiates a decimal by an integer without casting to float, maintaining maximum precision.
        /// </summary>
        /// <param name="decBase">Number to exponentiate.</param>
        /// <param name="intPower">Power to which to raise <paramref name="decBase"/>.</param>
        /// <returns><paramref name="decBase"/> to the power of <paramref name="intPower"/>.</returns>
        internal static decimal RaiseToPower(this decimal decBase, int intPower)
        {
            switch (intPower)
            {
                case 2: // Extremely common case, so handle it explicitly
                    return decBase * decBase;

                case 1:
                    return decBase;

                case 0: // Yes, even 0^0 should return 1 per IEEE specifications
                    return 1;

                case -1:
                    if (decBase == decimal.Zero)
                        throw new DivideByZeroException();
                    return 1.0m / decBase;
            }
            switch (decBase)
            {
                case decimal.One:
                    return 1;

                case decimal.Zero:
                    if (intPower < 0)
                        throw new DivideByZeroException();
                    return 0;

                case decimal.MinusOne:
                    return 1 - 2 * (Math.Abs(intPower) & 1);
            }
            // Handle negative powers by dividing by the result of the positive power right before we return
            bool blnNegativePower = intPower < 0;
            if (blnNegativePower)
                intPower = -intPower;
            decimal decReturn = 1;
            int i;
            // Dual loop structure looks funky, but cuts down on number of multiplication operations in worst case scenarios compared to a single loop
            for (; intPower > 1; intPower -= i / 2)
            {
                decimal decLoopElement = decBase;
                for (i = 2; i < intPower; i *= 2)
                {
                    decLoopElement *= decLoopElement;
                }
                decReturn *= decLoopElement;
            }

            return blnNegativePower ? 1.0m / decReturn : decReturn;
        }
    }
}
