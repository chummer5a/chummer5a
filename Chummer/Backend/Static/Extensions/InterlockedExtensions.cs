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

using System.Runtime.CompilerServices;
using System.Threading;
using System;
using System.Security;

namespace Chummer
{
    public static class InterlockedExtensions
    {
        /// <summary>
        /// Compares two enum values for equality and, if they are equal, replaces one of the values.
        /// Taken from the following (with some modifications): https://stackoverflow.com/a/59127914
        /// </summary>
        /// <param name="eLocation">The destination, whose value is compared with <paramref name="eComparand"/> and possibly replaced.</param>
        /// <param name="eValue">The value that replaces the destination value if the comparison results in equality.</param>
        /// <param name="eComparand">The value that is compared to the value at <paramref name="eLocation"/>.</param>
        /// <returns>The original value in <paramref name="eLocation"/>.</returns>
        /// <exception cref="System.NullReferenceException">The address of <paramref name="eLocation"/> is a null pointer</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SecuritySafeCritical]
        public static TEnum CompareExchange<TEnum>(ref TEnum eLocation, TEnum eValue, TEnum eComparand)
            where TEnum : struct, Enum
        {
            switch (Unsafe.SizeOf<TEnum>())
            {
                // .NET does not support 1- and 2-byte atomic operations as there
                // is no common hardware support for that.
                case 4:
                    return CompareExchange32Bit(ref eLocation, eValue, eComparand);
                case 8:
                    return CompareExchange64Bit(ref eLocation, eValue, eComparand);
                default:
                    throw new NotSupportedException(
                        "Only enums with an underlying type of 4 bytes or 8 bytes are allowed to be used with Interlocked");
            }

            TEnum CompareExchange32Bit(ref TEnum eInnerLocation, TEnum eInnerValue, TEnum eInnerComparand)
            {
                int intComparandRaw = Unsafe.As<TEnum, int>(ref eInnerComparand);
                int intValueRaw = Unsafe.As<TEnum, int>(ref eInnerValue);
                ref int intLocationRaw = ref Unsafe.As<TEnum, int>(ref eInnerLocation);
                int intReturnRaw = Interlocked.CompareExchange(ref intLocationRaw, intValueRaw, intComparandRaw);
                return Unsafe.As<int, TEnum>(ref intReturnRaw);
            }

            TEnum CompareExchange64Bit(ref TEnum eInnerLocation, TEnum eInnerValue, TEnum eInnerComparand)
            {
                long intComparandRaw = Unsafe.As<TEnum, long>(ref eInnerComparand);
                long intValueRaw = Unsafe.As<TEnum, long>(ref eInnerValue);
                ref long intLocationRaw = ref Unsafe.As<TEnum, long>(ref eInnerLocation);
                long intReturnRaw = Interlocked.CompareExchange(ref intLocationRaw, intValueRaw, intComparandRaw);
                return Unsafe.As<long, TEnum>(ref intReturnRaw);
            }
        }

        /// <summary>
        /// Sets an enum to a specified value and returns the original value, as an atomic operation.
        /// Taken from the following (with some modifications): https://stackoverflow.com/a/59127914
        /// </summary>
        /// <param name="eLocation">The variable to set to the specified value.</param>
        /// <param name="eValue">The value to which the <paramref name="eLocation"/> parameter is set.</param>
        /// <returns>The original value in <paramref name="eLocation"/>.</returns>
        /// <exception cref="System.NullReferenceException">The address of <paramref name="eLocation"/> is a null pointer</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SecuritySafeCritical]
        public static TEnum Exchange<TEnum>(ref TEnum eLocation, TEnum eValue)
            where TEnum : struct, Enum
        {
            switch (Unsafe.SizeOf<TEnum>())
            {
                // .NET does not support 1- and 2-byte atomic operations as there
                // is no common hardware support for that.
                case 4:
                    return Exchange32Bit(ref eLocation, eValue);
                case 8:
                    return Exchange64Bit(ref eLocation, eValue);
                default:
                    throw new NotSupportedException(
                        "Only enums with an underlying type of 4 bytes or 8 bytes are allowed to be used with Interlocked");
            }

            TEnum Exchange32Bit(ref TEnum eInnerLocation, TEnum eInnerValue)
            {
                int intValueRaw = Unsafe.As<TEnum, int>(ref eInnerValue);
                ref int intLocationRaw = ref Unsafe.As<TEnum, int>(ref eInnerLocation);
                int intReturnRaw = Interlocked.Exchange(ref intLocationRaw, intValueRaw);
                return Unsafe.As<int, TEnum>(ref intReturnRaw);
            }

            TEnum Exchange64Bit(ref TEnum eInnerLocation, TEnum eInnerValue)
            {
                long intValueRaw = Unsafe.As<TEnum, long>(ref eInnerValue);
                ref long intLocationRaw = ref Unsafe.As<TEnum, long>(ref eInnerLocation);
                long intReturnRaw = Interlocked.Exchange(ref intLocationRaw, intValueRaw);
                return Unsafe.As<long, TEnum>(ref intReturnRaw);
            }
        }
    }
}
