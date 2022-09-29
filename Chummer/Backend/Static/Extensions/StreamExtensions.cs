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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Array = System.Array;

namespace Chummer
{
    public static class StreamExtensions
    {
        public static byte[] ToPooledArray(this MemoryStream objStream)
        {
            objStream.Seek(0, SeekOrigin.Begin);
            int intLength = Convert.ToInt32(objStream.Length);
            byte[] achrReturn = ArrayPool<byte>.Shared.Rent(intLength);
            try
            {
                Array.Clear(achrReturn, 0, achrReturn.Length);
                _ = objStream.Read(achrReturn, 0, intLength);
            }
            catch
            {
                ArrayPool<byte>.Shared.Return(achrReturn);
                throw;
            }
            return achrReturn;
        }

        public static async Task<byte[]> ToPooledArrayAsync(this MemoryStream objStream, CancellationToken token = default)
        {
            objStream.Seek(0, SeekOrigin.Begin);
            int intLength = Convert.ToInt32(objStream.Length);
            byte[] achrReturn = ArrayPool<byte>.Shared.Rent(intLength);
            try
            {
                Array.Clear(achrReturn, 0, achrReturn.Length);
                _ = await objStream.ReadAsync(achrReturn, 0, intLength, token).ConfigureAwait(false);
            }
            catch
            {
                ArrayPool<byte>.Shared.Return(achrReturn);
                throw;
            }
            return achrReturn;
        }
    }
}
