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
using System.Runtime.CompilerServices;

namespace Chummer
{
    /// <summary>
    /// Syntactic Sugar for wrapping a ArrayPool{T}'s Rent() and Return() methods into something that hooks into `using`
    /// and that guarantees that pooled objects will be returned
    /// </summary>
    public readonly struct FetchSafelyFromArrayPool<T> : IDisposable, IEquatable<FetchSafelyFromArrayPool<T>>
    {
        private readonly ArrayPool<T> _objMyArrayPool;
        private readonly T[] _objMyArrayValue;

        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FetchSafelyFromArrayPool(ArrayPool<T> objMyArrayPool, int intMinimumSize, out T[] objReturn)
        {
            _objMyArrayPool = objMyArrayPool;
            objReturn = _objMyArrayValue = objMyArrayPool.Rent(intMinimumSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _objMyArrayPool?.Return(_objMyArrayValue);
        }

        public bool Equals(FetchSafelyFromArrayPool<T> other)
        {
            return _objMyArrayPool.Equals(other._objMyArrayPool) && _objMyArrayValue.Equals(other._objMyArrayValue);
        }

        public override bool Equals(object obj)
        {
            return obj is FetchSafelyFromArrayPool<T> objCasted && Equals(objCasted);
        }
        public static bool operator ==(FetchSafelyFromArrayPool<T> left, FetchSafelyFromArrayPool<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FetchSafelyFromArrayPool<T> left, FetchSafelyFromArrayPool<T> right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return (_objMyArrayPool, _objMyArrayValue).GetHashCode();
        }
    }
}
