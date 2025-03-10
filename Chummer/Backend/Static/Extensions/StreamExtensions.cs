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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Array = System.Array;

namespace Chummer
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Similar to Stream.ToArray(), but allocates to a rented array from ArrayPool instead of to a newly allocated array.
        /// </summary>
        /// <param name="objStream">Stream to convert to a byte array.</param>
        public static byte[] ToPooledArray(this Stream objStream)
        {
            if (objStream == null)
            {
                throw new ArgumentNullException(nameof(objStream));
            }
            objStream.Position = 0;
            int intLength = Convert.ToInt32(objStream.Length);
            byte[] achrReturn = ArrayPool<byte>.Shared.Rent(intLength);
            try
            {
                Array.Clear(achrReturn, 0, intLength);
                _ = objStream.Read(achrReturn, 0, intLength);
            }
            catch
            {
                ArrayPool<byte>.Shared.Return(achrReturn);
                throw;
            }
            return achrReturn;
        }

        /// <summary>
        /// Similar to Stream.ToArray(), but allocates to a rented array from ArrayPool instead of to a newly allocated array.
        /// </summary>
        /// <param name="objStream">Stream to convert to a byte array.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Task<byte[]> ToPooledArrayAsync(this Stream objStream, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objStream == null)
            {
                throw new ArgumentNullException(nameof(objStream));
            }
            return ToPooledArrayAsyncInner();
            async Task<byte[]> ToPooledArrayAsyncInner()
            {
                objStream.Position = 0;
                int intLength = Convert.ToInt32(objStream.Length);
                byte[] achrReturn = ArrayPool<byte>.Shared.Rent(intLength);
                try
                {
                    Array.Clear(achrReturn, 0, intLength);
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

        private static readonly char[] s_Base64Table = {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
            'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
            'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd',
            'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n',
            'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
            'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7',
            '8', '9', '+', '/', '='
        };

        /// <summary>
        /// Converts a stream directly to a Base64-encoded string without needing to allocate a byte array as an intermediate.
        /// More memory efficient than using some byte array converter on the stream followed by Convert.ToBase64String().
        /// </summary>
        /// <param name="objStream">Some stream to convert.</param>
        /// <param name="eFormattingOptions">Base64 formatting options to use in the output string.</param>
        /// <param name="token">Cancellation token to listen to, if any.</param>
        /// <returns>The string representation, in base 64, of the contents of <paramref name="objStream"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="objStream"/> is null.</exception>
        /// <exception cref="OutOfMemoryException"><paramref name="objStream"/> is too large to be converted to a base64-encoded string.</exception>
        public static string ToBase64String(this Stream objStream,
                                            Base64FormattingOptions eFormattingOptions = Base64FormattingOptions.None,
                                            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objStream == null)
            {
                throw new ArgumentNullException(nameof(objStream));
            }

            long intLength = objStream.Length;
            if (intLength == 0)
            {
                return string.Empty;
            }

            bool blnInsertLineBreaks = eFormattingOptions == Base64FormattingOptions.InsertLineBreaks;
            int intStringLength = ToBase64_CalculateAndValidateOutputLength(intLength, blnInsertLineBreaks);
            token.ThrowIfCancellationRequested();
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
            {
                sbdReturn.Capacity = intStringLength;
                _ = ConvertToBase64Array(sbdReturn, objStream, 0, (int) intLength, blnInsertLineBreaks);
                return sbdReturn.ToString();
            }

            int ToBase64_CalculateAndValidateOutputLength(long inputLength, bool insertLineBreaks)
            {
                long num = inputLength.DivRem(3, out long lngRemainder) * 4;
                if (lngRemainder != 0)
                    ++num;
                if (num == 0L)
                {
                    return 0;
                }

                if (insertLineBreaks)
                {
                    long num2 = num.DivRem(76, out lngRemainder);
                    if (lngRemainder == 0L)
                    {
                        num2--;
                    }

                    num += num2 * 2;
                }

                if (num > int.MaxValue)
                {
                    throw new OutOfMemoryException();
                }

                return (int) num;
            }

            unsafe int ConvertToBase64Array(StringBuilder sbdChars, Stream inData, int offset, int length,
                                            bool insertLineBreaks)
            {
                int num = length % 3;
                int num2 = offset + (length - num);
                int num3 = 0;
                int num4 = 0;
                fixed (char* ptr = s_Base64Table)
                {
                    inData.Position = offset;
                    byte[] achrBuffer = ArrayPool<byte>.Shared.Rent(3);
                    try
                    {
                        while (inData.Position < num2)
                        {
                            token.ThrowIfCancellationRequested();
                            _ = inData.Read(achrBuffer, 0, 3);

                            if (insertLineBreaks)
                            {
                                if (num4 == 76)
                                {
                                    sbdChars.Append("\r\n");
                                    num3 += 2;
                                    num4 = 0;
                                }

                                num4 += 4;
                            }

                            sbdChars.Append(ptr[(achrBuffer[0] & 0xFC) >> 2])
                                .Append(ptr[((achrBuffer[0] & 3) << 4) | ((achrBuffer[1] & 0xF0) >> 4)])
                                .Append(ptr[((achrBuffer[1] & 0xF) << 2) | ((achrBuffer[2] & 0xC0) >> 6)])
                                .Append(ptr[achrBuffer[2] & 0x3F]);
                            num3 += 4;
                        }

                        token.ThrowIfCancellationRequested();

                        if (insertLineBreaks && num != 0 && num4 == 76)
                        {
                            sbdChars.Append("\r\n");
                            num3 += 2;
                        }

                        switch (num)
                        {
                            case 2:
                            {
                                _ = inData.Read(achrBuffer, 0, 2);
                                sbdChars.Append(ptr[(achrBuffer[0] & 0xFC) >> 2])
                                    .Append(ptr[((achrBuffer[0] & 3) << 4) | ((achrBuffer[1] & 0xF0) >> 4)])
                                    .Append(ptr[(achrBuffer[1] & 0xF) << 2])
                                    .Append(ptr[64]);
                                num3 += 4;
                                break;
                            }
                            case 1:
                            {
                                _ = inData.Read(achrBuffer, 0, 1);
                                sbdChars.Append(ptr[(achrBuffer[0] & 0xFC) >> 2])
                                    .Append(ptr[(achrBuffer[0] & 3) << 4])
                                    .Append(ptr[64])
                                    .Append(ptr[64]);
                                num3 += 4;
                                break;
                            }
                        }
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(achrBuffer);
                    }
                }

                return num3;
            }
        }

        /// <summary>
        /// Converts a stream directly to a Base64-encoded string without needing to allocate a byte array as an intermediate.
        /// More memory efficient than using some byte array converter on the stream followed by Convert.ToBase64String().
        /// </summary>
        /// <param name="objStream">Some stream to convert.</param>
        /// <param name="eFormattingOptions">Base64 formatting options to use in the output string.</param>
        /// <param name="token">Cancellation token to listen to, if any.</param>
        /// <returns>The string representation, in base 64, of the contents of <paramref name="objStream"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="objStream"/> is null.</exception>
        /// <exception cref="OutOfMemoryException"><paramref name="objStream"/> is too large to be converted to a base64-encoded string.</exception>
        public static async Task<string> ToBase64StringAsync(this Stream objStream,
                                            Base64FormattingOptions eFormattingOptions = Base64FormattingOptions.None,
                                            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objStream == null)
            {
                throw new ArgumentNullException(nameof(objStream));
            }

            long intLength = objStream.Length;
            if (intLength == 0)
            {
                return string.Empty;
            }

            bool blnInsertLineBreaks = eFormattingOptions == Base64FormattingOptions.InsertLineBreaks;
            int intStringLength = ToBase64_CalculateAndValidateOutputLength(intLength, blnInsertLineBreaks);
            token.ThrowIfCancellationRequested();
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
            {
                sbdReturn.Capacity = intStringLength;
                _ = await ConvertToBase64ArrayAsync(sbdReturn, objStream, 0, (int)intLength, blnInsertLineBreaks).ConfigureAwait(false);
                return sbdReturn.ToString();
            }

            int ToBase64_CalculateAndValidateOutputLength(long inputLength, bool insertLineBreaks)
            {
                long num = inputLength.DivRem(3, out long lngRemainder) * 4;
                if (lngRemainder != 0)
                    ++num;
                if (num == 0L)
                {
                    return 0;
                }

                if (insertLineBreaks)
                {
                    long num2 = num.DivRem(76, out lngRemainder);
                    if (lngRemainder == 0L)
                    {
                        num2--;
                    }

                    num += num2 * 2;
                }

                if (num > int.MaxValue)
                {
                    throw new OutOfMemoryException();
                }

                return (int)num;
            }

            async ValueTask<int> ConvertToBase64ArrayAsync(StringBuilder sbdChars, Stream inData, int offset,
                int length,
                bool insertLineBreaks)
            {
                int num = length % 3;
                int num2 = offset + (length - num);
                int num3 = 0;
                int num4 = 0;
                inData.Position = offset;
                byte[] achrBuffer = ArrayPool<byte>.Shared.Rent(3);
                try
                {
                    while (inData.Position < num2)
                    {
                        token.ThrowIfCancellationRequested();
                        _ = await inData.ReadAsync(achrBuffer, 0, 3, token).ConfigureAwait(false);

                        if (insertLineBreaks)
                        {
                            if (num4 == 76)
                            {
                                sbdChars.Append("\r\n");
                                num3 += 2;
                                num4 = 0;
                            }

                            num4 += 4;
                        }

                        sbdChars.Append(s_Base64Table[(achrBuffer[0] & 0xFC) >> 2])
                            .Append(s_Base64Table[((achrBuffer[0] & 3) << 4) | ((achrBuffer[1] & 0xF0) >> 4)])
                            .Append(s_Base64Table[((achrBuffer[1] & 0xF) << 2) | ((achrBuffer[2] & 0xC0) >> 6)])
                            .Append(s_Base64Table[achrBuffer[2] & 0x3F]);
                        num3 += 4;
                    }

                    token.ThrowIfCancellationRequested();

                    if (insertLineBreaks && num != 0 && num4 == 76)
                    {
                        sbdChars.Append("\r\n");
                        num3 += 2;
                    }

                    switch (num)
                    {
                        case 2:
                        {
                            _ = await inData.ReadAsync(achrBuffer, 0, 2, token).ConfigureAwait(false);
                            sbdChars.Append(s_Base64Table[(achrBuffer[0] & 0xFC) >> 2])
                                .Append(s_Base64Table[((achrBuffer[0] & 3) << 4) | ((achrBuffer[1] & 0xF0) >> 4)])
                                .Append(s_Base64Table[(achrBuffer[1] & 0xF) << 2])
                                .Append(s_Base64Table[64]);
                            num3 += 4;
                            break;
                        }
                        case 1:
                        {
                            _ = await inData.ReadAsync(achrBuffer, 0, 1, token).ConfigureAwait(false);
                            sbdChars.Append(s_Base64Table[(achrBuffer[0] & 0xFC) >> 2])
                                .Append(s_Base64Table[(achrBuffer[0] & 3) << 4])
                                .Append(s_Base64Table[64])
                                .Append(s_Base64Table[64]);
                            num3 += 4;
                            break;
                        }
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(achrBuffer);
                }

                return num3;
            }
        }
    }
}
