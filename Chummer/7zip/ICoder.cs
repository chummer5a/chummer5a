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
// ICoder.h

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SevenZip
{
    /// <summary>
    /// The exception that is thrown when an error in input stream occurs during decoding.
    /// </summary>
    internal sealed class DataErrorException : ApplicationException
    {
        public DataErrorException() : base("Data Error")
        {
        }
    }

    /// <summary>
    /// The exception that is thrown when the value of an argument is outside the allowable range.
    /// </summary>
    internal sealed class InvalidParamException : ApplicationException
    {
        public InvalidParamException() : base("Invalid Parameter")
        {
        }
    }

    public interface ICodeProgress
    {
        /// <summary>
        /// Callback progress.
        /// </summary>
        /// <param name="inSize">
        /// input size. -1 if unknown.
        /// </param>
        /// <param name="outSize">
        /// output size. -1 if unknown.
        /// </param>
        void SetProgress(long inSize, long outSize);
    }

    public interface IAsyncCodeProgress
    {
        /// <summary>
        /// (Asynchronous) Callback progress.
        /// </summary>
        /// <param name="inSize">
        /// input size. -1 if unknown.
        /// </param>
        /// <param name="outSize">
        /// output size. -1 if unknown.
        /// </param>
        /// <param name="token">
        /// cancellation token to use for the callback.
        /// </param>
        Task SetProgressAsync(long inSize, long outSize, CancellationToken token = default);
    }

    public interface ICoder
    {
        /// <summary>
        /// Codes streams.
        /// </summary>
        /// <param name="inStream">
        /// input Stream.
        /// </param>
        /// <param name="outStream">
        /// output Stream.
        /// </param>
        /// <param name="inSize">
        /// input Size. -1 if unknown.
        /// </param>
        /// <param name="outSize">
        /// output Size. -1 if unknown.
        /// </param>
        /// <param name="progress">
        /// callback progress reference.
        /// </param>
        /// <exception cref="SevenZip.DataErrorException">
        /// if input stream is not valid
        /// </exception>
        void Code(System.IO.Stream inStream, System.IO.Stream outStream,
            long inSize, long outSize, ICodeProgress progress);

        /// <summary>
        /// Codes streams asynchronously.
        /// </summary>
        /// <param name="inStream">
        /// input Stream.
        /// </param>
        /// <param name="outStream">
        /// output Stream.
        /// </param>
        /// <param name="inSize">
        /// input Size. -1 if unknown.
        /// </param>
        /// <param name="outSize">
        /// output Size. -1 if unknown.
        /// </param>
        /// <param name="progress">
        /// callback progress reference.
        /// </param>
        /// <param name="token">
        /// cancellation token to use for the coding.
        /// </param>
        /// <exception cref="SevenZip.DataErrorException">
        /// if input stream is not valid
        /// </exception>
        Task CodeAsync(System.IO.Stream inStream, System.IO.Stream outStream,
                  long inSize, long outSize, IAsyncCodeProgress progress, CancellationToken token = default);
    }

    /*
	public interface ICoder2
	{
		 void Code(ISequentialInStream []inStreams,
				const UInt64 []inSizes,
				ISequentialOutStream []outStreams,
				UInt64 []outSizes,
				ICodeProgress progress);
	}
    */

    /// <summary>
    /// Provides the fields that represent properties identifiers for compressing.
    /// </summary>
    public enum CoderPropID
    {
        /// <summary>
        /// Specifies default property.
        /// </summary>
        DefaultProp = 0,

        /// <summary>
        /// Specifies size of dictionary.
        /// </summary>
        DictionarySize,

        /// <summary>
        /// Specifies size of memory for PPM*.
        /// </summary>
        UsedMemorySize,

        /// <summary>
        /// Specifies order for PPM methods.
        /// </summary>
        Order,

        /// <summary>
        /// Specifies Block Size.
        /// </summary>
        BlockSize,

        /// <summary>
        /// Specifies number of position state bits for LZMA (0 &lt;= x &lt;= 4).
        /// </summary>
        PosStateBits,

        /// <summary>
        /// Specifies number of literal context bits for LZMA (0 &lt;= x &lt;= 8).
        /// </summary>
        LitContextBits,

        /// <summary>
        /// Specifies number of literal position bits for LZMA (0 &lt;= x &lt;= 4).
        /// </summary>
        LitPosBits,

        /// <summary>
        /// Specifies number of fast bytes for LZ*.
        /// </summary>
        NumFastBytes,

        /// <summary>
        /// Specifies match finder. LZMA: "BT2", "BT4" or "BT4B".
        /// </summary>
        MatchFinder,

        /// <summary>
        /// Specifies the number of match finder cycles.
        /// </summary>
        MatchFinderCycles,

        /// <summary>
        /// Specifies number of passes.
        /// </summary>
        NumPasses,

        /// <summary>
        /// Specifies number of algorithm.
        /// </summary>
        Algorithm,

        /// <summary>
        /// Specifies the number of threads.
        /// </summary>
        NumThreads,

        /// <summary>
        /// Specifies mode with end marker.
        /// </summary>
        EndMarker
    }

    public interface ISetCoderProperties
    {
        void SetCoderProperties(CoderPropID[] propIDs, object[] properties);
    }

    public interface IWriteCoderProperties
    {
        void WriteCoderProperties(System.IO.Stream outStream);

        Task WriteCoderPropertiesAsync(System.IO.Stream outStream, CancellationToken token = default);
    }

    public interface ISetDecoderProperties
    {
        void SetDecoderProperties(byte[] properties);
    }
}
