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
using System.Runtime.ConstrainedExecution;
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
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
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

        /// <inheritdoc cref="Interlocked.CompareExchange(ref long, long, long)"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SecuritySafeCritical]
        [CLSCompliant(false)]
        public static ulong CompareExchange(ref ulong lngLocation, ulong lngValue, ulong lngComparand)
        {
            long intComparandRaw = Unsafe.As<ulong, long>(ref lngComparand);
            long intValueRaw = Unsafe.As<ulong, long>(ref lngValue);
            ref long intLocationRaw = ref Unsafe.As<ulong, long>(ref lngLocation);
            long intReturnRaw = Interlocked.CompareExchange(ref intLocationRaw, intValueRaw, intComparandRaw);
            return Unsafe.As<long, ulong>(ref intReturnRaw);
        }

        /// <inheritdoc cref="Interlocked.CompareExchange(ref int, int, int)"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SecuritySafeCritical]
        [CLSCompliant(false)]
        public static uint CompareExchange(ref uint intLocation, uint intValue, uint intComparand)
        {
            int intComparandRaw = Unsafe.As<uint, int>(ref intComparand);
            int intValueRaw = Unsafe.As<uint, int>(ref intValue);
            ref int intLocationRaw = ref Unsafe.As<uint, int>(ref intLocation);
            int intReturnRaw = Interlocked.CompareExchange(ref intLocationRaw, intValueRaw, intComparandRaw);
            return Unsafe.As<int, uint>(ref intReturnRaw);
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
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
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

        /// <inheritdoc cref="Interlocked.Exchange(ref long, long)"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SecuritySafeCritical]
        [CLSCompliant(false)]
        public static ulong Exchange(ref ulong lngLocation, ulong lngValue)
        {
            long lngValueRaw = Unsafe.As<ulong, long>(ref lngValue);
            ref long lngLocationRaw = ref Unsafe.As<ulong, long>(ref lngLocation);
            long lngReturnRaw = Interlocked.Exchange(ref lngLocationRaw, lngValueRaw);
            return Unsafe.As<long, ulong>(ref lngReturnRaw);
        }

        /// <inheritdoc cref="Interlocked.Exchange(ref int, int)"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SecuritySafeCritical]
        [CLSCompliant(false)]
        public static uint Exchange(ref uint intLocation, uint intValue)
        {
            int intValueRaw = Unsafe.As<uint, int>(ref intValue);
            ref int intLocationRaw = ref Unsafe.As<uint, int>(ref intLocation);
            int intReturnRaw = Interlocked.Exchange(ref intLocationRaw, intValueRaw);
            return Unsafe.As<int, uint>(ref intReturnRaw);
        }

        /// <inheritdoc cref="Interlocked.Add(ref long, long)"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SecuritySafeCritical]
        [CLSCompliant(false)]
        public static ulong Add(ref ulong lngLocation, ulong lngValue)
        {
            long lngValueRaw = Unsafe.As<ulong, long>(ref lngValue);
            ref long lngLocationRaw = ref Unsafe.As<ulong, long>(ref lngLocation);
            unchecked
            {
                long lngReturnRaw = Interlocked.Add(ref lngLocationRaw, lngValueRaw);
                return Unsafe.As<long, ulong>(ref lngReturnRaw);
            }
        }

        /// <inheritdoc cref="Interlocked.Add(ref int, int)"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SecuritySafeCritical]
        [CLSCompliant(false)]
        public static uint Add(ref uint intLocation, uint intValue)
        {
            int intValueRaw = Unsafe.As<uint, int>(ref intValue);
            ref int intLocationRaw = ref Unsafe.As<uint, int>(ref intLocation);
            unchecked
            {
                int intReturnRaw = Interlocked.Add(ref intLocationRaw, intValueRaw);
                return Unsafe.As<int, uint>(ref intReturnRaw);
            }
        }

        /// <inheritdoc cref="Interlocked.Increment(ref long)"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SecuritySafeCritical]
        [CLSCompliant(false)]
        public static ulong Increment(ref ulong lngLocation)
        {
            ref long lngLocationRaw = ref Unsafe.As<ulong, long>(ref lngLocation);
            unchecked
            {
                long lngReturnRaw = Interlocked.Increment(ref lngLocationRaw);
                return Unsafe.As<long, ulong>(ref lngReturnRaw);
            }
        }

        /// <inheritdoc cref="Interlocked.Increment(ref int)"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SecuritySafeCritical]
        [CLSCompliant(false)]
        public static uint Increment(ref uint intLocation)
        {
            ref int intLocationRaw = ref Unsafe.As<uint, int>(ref intLocation);
            unchecked
            {
                int intReturnRaw = Interlocked.Increment(ref intLocationRaw);
                return Unsafe.As<int, uint>(ref intReturnRaw);
            }
        }

        /// <inheritdoc cref="Interlocked.Decrement(ref long)"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SecuritySafeCritical]
        [CLSCompliant(false)]
        public static ulong Decrement(ref ulong lngLocation)
        {
            ref long lngLocationRaw = ref Unsafe.As<ulong, long>(ref lngLocation);
            unchecked
            {
                long lngReturnRaw = Interlocked.Decrement(ref lngLocationRaw);
                return Unsafe.As<long, ulong>(ref lngReturnRaw);
            }
        }

        /// <inheritdoc cref="Interlocked.Decrement(ref int)"/>>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SecuritySafeCritical]
        [CLSCompliant(false)]
        public static uint Decrement(ref uint intLocation)
        {
            ref int intLocationRaw = ref Unsafe.As<uint, int>(ref intLocation);
            unchecked
            {
                int intReturnRaw = Interlocked.Decrement(ref intLocationRaw);
                return Unsafe.As<int, uint>(ref intReturnRaw);
            }
        }
    }
}
