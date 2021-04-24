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
        /// Syntatic sugar for doing integer division that always rounds away from zero instead of towards zero.
        /// </summary>
        /// <param name="intA">Dividend integer.</param>
        /// <param name="intB">Divisor integer.</param>
        /// <returns><paramref name="intA"/> divided by <paramref name="intB"/>, rounded towards the nearest number away from zero (up if positive, down if negative).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int DivAwayFromZero(this int intA, int intB)
        {
            return (intA + Math.Sign(intA) * (Math.Abs(intB) - 1)) / intB; // Adding 1 if modulo > 0 would require a separate modulo operation that is as slow as division
        }
    }
}
