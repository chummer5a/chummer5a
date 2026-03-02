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
using System.Security;

namespace Chummer
{
    public static class BoolExtensions
    {
        /// <summary>
        /// Converts a boolean to number form the fastest way possible.
        /// </summary>
        /// <param name="value">The boolean to convert</param>
        /// <returns>1 if <paramref name="value"/> is True, 0 if <paramref name="value"/> is False.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SecuritySafeCritical]
        public static unsafe long ToInt64(this bool value)
        {
            return (*(byte*)&value != 0).ToInt64NonNormalized();
        }

        /// <summary>
        /// Converts a boolean to number form the fastest way possible.
        /// </summary>
        /// <param name="value">The boolean to convert</param>
        /// <returns>1 if <paramref name="value"/> is True, 0 if <paramref name="value"/> is False.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SecuritySafeCritical]
        public static unsafe int ToInt32(this bool value)
        {
            return (*(byte*)&value != 0).ToInt32NonNormalized();
        }

        /// <summary>
        /// Converts a boolean to number form the fastest way possible.
        /// </summary>
        /// <param name="value">The boolean to convert</param>
        /// <returns>1 if <paramref name="value"/> is True, 0 if <paramref name="value"/> is False.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SecuritySafeCritical]
        public static unsafe short ToShort(this bool value)
        {
            return (*(byte*)&value != 0).ToShortNonNormalized();
        }

        /// <summary>
        /// Converts a boolean to number form the fastest way possible.
        /// </summary>
        /// <param name="value">The boolean to convert</param>
        /// <returns>1 if <paramref name="value"/> is True, 0 if <paramref name="value"/> is False.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SecuritySafeCritical]
        public static unsafe byte ToByte(this bool value)
        {
            return (*(byte*)&value != 0).ToByteNonNormalized();
        }

        /// <summary>
        /// Converts a boolean to number form the fastest way possible (interop-unsafe version).
        /// </summary>
        /// <param name="value">The boolean to convert</param>
        /// <returns>1 if <paramref name="value"/> is True, 0 if <paramref name="value"/> is False.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SecuritySafeCritical]
        private static unsafe long ToInt64NonNormalized(this bool value)
        {
            return *(byte*)&value;
        }

        /// <summary>
        /// Converts a boolean to number form the fastest way possible (interop-unsafe version).
        /// </summary>
        /// <param name="value">The boolean to convert</param>
        /// <returns>1 if <paramref name="value"/> is True, 0 if <paramref name="value"/> is False.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SecuritySafeCritical]
        private static unsafe int ToInt32NonNormalized(this bool value)
        {
            return *(byte*)&value;
        }

        /// <summary>
        /// Converts a boolean to number form the fastest way possible (interop-unsafe version).
        /// </summary>
        /// <param name="value">The boolean to convert</param>
        /// <returns>1 if <paramref name="value"/> is True, 0 if <paramref name="value"/> is False.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SecuritySafeCritical]
        private static unsafe short ToShortNonNormalized(this bool value)
        {
            return *(byte*)&value;
        }

        /// <summary>
        /// Converts a boolean to number form the fastest way possible (interop-unsafe version).
        /// </summary>
        /// <param name="value">The boolean to convert</param>
        /// <returns>1 if <paramref name="value"/> is True, 0 if <paramref name="value"/> is False.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SecuritySafeCritical]
        private static unsafe byte ToByteNonNormalized(this bool value)
        {
            return *(byte*)&value;
        }
    }
}
