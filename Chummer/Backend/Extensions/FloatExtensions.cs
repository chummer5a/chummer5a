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
    public static class FloatExtensions
    {
        /// <summary>
        /// Syntactic sugar for rounding a float away from zero and then converting it to an integer.
        /// </summary>
        /// <param name="fltToRound">Float to round.</param>
        /// <returns>Rounded integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int StandardRound(this float fltToRound)
        {
            return Convert.ToInt32(fltToRound >= 0 ? Math.Ceiling(fltToRound) : Math.Floor(fltToRound));
        }

        /// <summary>
        /// Syntactic sugar for exponentiating a float by another float.
        /// </summary>
        /// <param name="fltBase">Number to exponentiate.</param>
        /// <param name="fltPower">Power to which to raise <paramref name="fltBase"/>.</param>
        /// <returns><paramref name="fltBase"/> to the power of <paramref name="fltPower"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static float RaiseToPower(this float fltBase, float fltPower)
        {
            return Convert.ToSingle(Math.Pow(fltBase, fltPower));
        }
    }
}
