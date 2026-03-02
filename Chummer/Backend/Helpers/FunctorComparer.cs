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
using System.Collections.Generic;

namespace Chummer
{
    /// <summary>
    /// Public version of the helper used by Array.Sort methods to turn Comparisons into IComparers.
    /// </summary>
    public readonly struct FunctorComparer<T> : IComparer<T>, IEquatable<FunctorComparer<T>>
    {
        private readonly Comparison<T> _funcComparison;

        public FunctorComparer(Comparison<T> funcComparison)
        {
            _funcComparison = funcComparison;
        }

        public int Compare(T x, T y)
        {
            return _funcComparison(x, y);
        }

        public bool Equals(FunctorComparer<T> other)
        {
            return _funcComparison == other._funcComparison;
        }

        public override bool Equals(object obj)
        {
            return obj is FunctorComparer<T> objCast && Equals(objCast);
        }
        public static bool operator ==(FunctorComparer<T> left, FunctorComparer<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FunctorComparer<T> left, FunctorComparer<T> right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return _funcComparison.GetHashCode();
        }
    }
}
