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
    public static class DoubleExtensions
    {
        /// <summary>
        /// Syntactic sugar for rounding a double away from zero and then converting it to an integer.
        /// </summary>
        /// <param name="dblToRound">Double to round.</param>
        /// <returns>Rounded integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int StandardRound(this double dblToRound)
        {
            return Convert.ToInt32(dblToRound >= 0 ? Math.Ceiling(dblToRound) : Math.Floor(dblToRound));
        }

        /// <summary>
        /// Syntactic sugar for exponentiating a double by another double.
        /// </summary>
        /// <param name="dblBase">Number to exponentiate.</param>
        /// <param name="dblPower">Power to which to raise <paramref name="dblBase"/>.</param>
        /// <returns><paramref name="dblBase"/> to the power of <paramref name="dblPower"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static double RaiseToPower(this double dblBase, double dblPower)
        {
            return Math.Pow(dblBase, dblPower);
        }
    }
}
