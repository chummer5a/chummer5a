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
    /// <summary>
    /// Syntactic Sugar for wrapping <see cref="SafeObjectPool{T}.Get"/> and <see cref="SafeObjectPool{T}.Return"/> into something that hooks into `using`
    /// and that guarantees that pooled objects will be returned
    /// </summary>
    public struct FetchSafelyFromSafeObjectPool<T> : IDisposable, IEquatable<FetchSafelyFromSafeObjectPool<T>> where T : class // struct unfortunately cannot be readonly because SafeObjectPool.Return requires a reference argument
    {
        private readonly SafeObjectPool<T> _objMySafePool;
        private T _objMyValue;

        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FetchSafelyFromSafeObjectPool(SafeObjectPool<T> objMySafePool, out T objReturn)
        {
            ArgumentNullException.ThrowIfNull(objMySafePool, nameof(objMySafePool));
            _objMySafePool = objMySafePool;
            objReturn = _objMyValue = objMySafePool.Get();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _objMySafePool?.Return(ref _objMyValue);
        }

        public override bool Equals(object obj)
        {
            return Equals((FetchSafelyFromSafeObjectPool<T>)obj);
        }

        public override int GetHashCode()
        {
            return (_objMyValue, _objMySafePool).GetHashCode();
        }

        public static bool operator ==(FetchSafelyFromSafeObjectPool<T> left, FetchSafelyFromSafeObjectPool<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FetchSafelyFromSafeObjectPool<T> left, FetchSafelyFromSafeObjectPool<T> right)
        {
            return !(left == right);
        }

        public bool Equals(FetchSafelyFromSafeObjectPool<T> other)
        {
            return other._objMySafePool == _objMySafePool && other._objMyValue == _objMyValue;
        }
    }
}
