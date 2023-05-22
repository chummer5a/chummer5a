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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SevenZip;
using SevenZip.Compression.LZMA;

namespace Chummer
{
    public static class LzmaHelper
    {
        private static readonly Lazy<Encoder> s_LzyEncoder = new Lazy<Encoder>(() => new Encoder());
        private static readonly Lazy<Decoder> s_LzyDecoder = new Lazy<Decoder>(() => new Decoder());

        public enum SevenZipCompressionLevel
        {
            Store = 0,
            Fastest = 1,
            Fast = 3,
            Normal = 5,
            Maximum = 7,
            Ultra = 9
        }

        public enum ChummerCompressionPreset
        {
            Fast,
            Balanced,
            Thorough
        }

        public static void CompressToLzmaFile(this Stream objInStream, FileStream objOutStream,
                                              SevenZipCompressionLevel eSevenZipCompressionLevel,
                                              Action<long, long> funcOnProgress = null)
        {
            // ReSharper disable RedundantArgumentDefaultValue
            switch (eSevenZipCompressionLevel)
            {
                case SevenZipCompressionLevel.Store:
                    objInStream.CopyTo(objOutStream);
                    break;

                case SevenZipCompressionLevel.Fastest:
                    CompressToLzmaFile(objInStream, objOutStream, 8, 32, Encoder.EMatchFinderType.BT2);
                    break;

                case SevenZipCompressionLevel.Fast:
                    CompressToLzmaFile(objInStream, objOutStream, 22, 32, Encoder.EMatchFinderType.BT2);
                    break;

                case SevenZipCompressionLevel.Normal:
                    CompressToLzmaFile(objInStream, objOutStream, 24, 32, Encoder.EMatchFinderType.BT4);
                    break;

                case SevenZipCompressionLevel.Maximum:
                    CompressToLzmaFile(objInStream, objOutStream, 25, 64, Encoder.EMatchFinderType.BT4);
                    break;

                case SevenZipCompressionLevel.Ultra:
                    CompressToLzmaFile(objInStream, objOutStream, 26, 64, Encoder.EMatchFinderType.BT4);
                    break;

                default:
                    goto case SevenZipCompressionLevel.Normal;
            }
            // ReSharper restore RedundantArgumentDefaultValue
        }

        public static void CompressToLzmaFile(this Stream objInStream, FileStream objOutStream,
                                              ChummerCompressionPreset eChummerCompressionPreset,
                                              Action<long, long> funcOnProgress = null)
        {
            // ReSharper disable RedundantArgumentDefaultValue
            switch (eChummerCompressionPreset)
            {
                case ChummerCompressionPreset.Fast:
                    CompressToLzmaFile(objInStream, objOutStream, 22, 32, Encoder.EMatchFinderType.BT2);
                    break;

                case ChummerCompressionPreset.Balanced:
                    CompressToLzmaFile(objInStream, objOutStream, 24, 64, Encoder.EMatchFinderType.BT4);
                    break;

                case ChummerCompressionPreset.Thorough:
                    CompressToLzmaFile(objInStream, objOutStream, 26, 128, Encoder.EMatchFinderType.BT4);
                    break;

                default:
                    goto case ChummerCompressionPreset.Balanced;
            }
            // ReSharper restore RedundantArgumentDefaultValue
        }

        public static void CompressToLzmaFile(this Stream objInStream, FileStream objOutStream,
                                              int intCompressionLevel = Encoder.kDefaultDictionaryLogSize,
                                              int numFastBytes = (int)Encoder.kNumFastBytesDefault,
                                              Encoder.EMatchFinderType mf = Encoder.EMatchFinderType.BT4,
                                              Action<long, long> funcOnProgress = null)
        {
            if (intCompressionLevel < 0 || intCompressionLevel > 30)
                throw new ArgumentOutOfRangeException(nameof(intCompressionLevel));
            if (numFastBytes < 5 || numFastBytes > 255)
                throw new ArgumentOutOfRangeException(nameof(numFastBytes));
            int dictionary = 1 << intCompressionLevel;
            const int posStateBits = 2;
            const int litContextBits = 3; // for normal files
            // const uint litContextBits = 0; // for 32-bit data
            const int litPosBits = 0;
            // const uint litPosBits = 2; // for 32-bit data
            const int algorithm = 2;
            const bool eos = true;
            const bool stdInMode = false;

            CoderPropID[] propIDs =
            {
                CoderPropID.DictionarySize,
                CoderPropID.PosStateBits,
                CoderPropID.LitContextBits,
                CoderPropID.LitPosBits,
                CoderPropID.Algorithm,
                CoderPropID.NumFastBytes,
                CoderPropID.MatchFinder,
                CoderPropID.EndMarker
            };

            object[] properties =
            {
                dictionary,
                posStateBits,
                litContextBits,
                litPosBits,
                algorithm,
                numFastBytes,
                mf,
                eos
            };

            Encoder encoder = s_LzyEncoder.Value;
            encoder.SetCoderProperties(propIDs, properties);
            encoder.WriteCoderProperties(objOutStream);
            long fileSize = eos || stdInMode ? -1 : objInStream.Length;
            objOutStream.Write(BitConverter.GetBytes(fileSize), 0, 8);
            ICodeProgress funcProgress = funcOnProgress != null ? new DelegateCodeProgress(funcOnProgress) : null;
            encoder.Code(objInStream, objOutStream, -1, -1, funcProgress);
        }

        public static Task CompressToLzmaFileAsync(this Stream objInStream, FileStream objOutStream,
                                                   SevenZipCompressionLevel eSevenZipCompressionLevel,
                                                   Func<long, long, Task> funcOnProgress = null,
                                                   CancellationToken token = default)
        {
            // ReSharper disable RedundantArgumentDefaultValue
            switch (eSevenZipCompressionLevel)
            {
                case SevenZipCompressionLevel.Store:
                    return objInStream.CopyToAsync(objOutStream, 81920, token);

                case SevenZipCompressionLevel.Fastest:
                    return CompressToLzmaFileAsync(objInStream, objOutStream, 8, 32, Encoder.EMatchFinderType.BT2, funcOnProgress,
                                                   token);

                case SevenZipCompressionLevel.Fast:
                    return CompressToLzmaFileAsync(objInStream, objOutStream, 22, 32, Encoder.EMatchFinderType.BT2, funcOnProgress,
                                                   token);

                case SevenZipCompressionLevel.Normal:
                    return CompressToLzmaFileAsync(objInStream, objOutStream, 24, 32, Encoder.EMatchFinderType.BT4, funcOnProgress,
                                                   token);

                case SevenZipCompressionLevel.Maximum:
                    return CompressToLzmaFileAsync(objInStream, objOutStream, 25, 64, Encoder.EMatchFinderType.BT4, funcOnProgress,
                                                   token);

                case SevenZipCompressionLevel.Ultra:
                    return CompressToLzmaFileAsync(objInStream, objOutStream, 26, 64, Encoder.EMatchFinderType.BT4, funcOnProgress,
                                                   token);

                default:
                    goto case SevenZipCompressionLevel.Normal;
            }
            // ReSharper enable RedundantArgumentDefaultValue
        }

        public static Task CompressToLzmaFileAsync(this Stream objInStream, FileStream objOutStream,
                                                   ChummerCompressionPreset eChummerCompressionPreset,
                                                   Func<long, long, Task> funcOnProgress = null,
                                                   CancellationToken token = default)
        {
            // ReSharper disable RedundantArgumentDefaultValue
            switch (eChummerCompressionPreset)
            {
                case ChummerCompressionPreset.Fast:
                    return CompressToLzmaFileAsync(objInStream, objOutStream, 22, 32, Encoder.EMatchFinderType.BT2, funcOnProgress,
                                                   token);

                case ChummerCompressionPreset.Balanced:
                    return CompressToLzmaFileAsync(objInStream, objOutStream, 24, 64, Encoder.EMatchFinderType.BT4, funcOnProgress,
                                                   token);

                case ChummerCompressionPreset.Thorough:
                    return CompressToLzmaFileAsync(objInStream, objOutStream, 26, 128, Encoder.EMatchFinderType.BT4, funcOnProgress,
                                                   token);

                default:
                    goto case ChummerCompressionPreset.Balanced;
            }
            // ReSharper restore RedundantArgumentDefaultValue
        }

        public static async Task CompressToLzmaFileAsync(this Stream objInStream, FileStream objOutStream,
                                                   int intCompressionLevel = Encoder.kDefaultDictionaryLogSize,
                                                   int numFastBytes = (int)Encoder.kNumFastBytesDefault,
                                                   Encoder.EMatchFinderType mf = Encoder.EMatchFinderType.BT4,
                                                   Func<long, long, Task> funcOnProgress = null,
                                                   CancellationToken token = default)
        {
            if (intCompressionLevel < 0 || intCompressionLevel > 30)
                throw new ArgumentOutOfRangeException(nameof(intCompressionLevel));
            if (numFastBytes < 5 || numFastBytes > 255)
                throw new ArgumentOutOfRangeException(nameof(numFastBytes));
            int dictionary = 1 << intCompressionLevel;
            const int posStateBits = 2;
            const int litContextBits = 3; // for normal files
            // const uint litContextBits = 0; // for 32-bit data
            const int litPosBits = 0;
            // const uint litPosBits = 2; // for 32-bit data
            const int algorithm = 2;
            const bool eos = true;
            const bool stdInMode = false;

            token.ThrowIfCancellationRequested();

            CoderPropID[] propIDs =
            {
                CoderPropID.DictionarySize,
                CoderPropID.PosStateBits,
                CoderPropID.LitContextBits,
                CoderPropID.LitPosBits,
                CoderPropID.Algorithm,
                CoderPropID.NumFastBytes,
                CoderPropID.MatchFinder,
                CoderPropID.EndMarker
            };

            object[] properties =
            {
                dictionary,
                posStateBits,
                litContextBits,
                litPosBits,
                algorithm,
                numFastBytes,
                mf,
                eos
            };

            Encoder encoder = s_LzyEncoder.Value;
            token.ThrowIfCancellationRequested();
            encoder.SetCoderProperties(propIDs, properties);
            await encoder.WriteCoderPropertiesAsync(objOutStream, token).ConfigureAwait(false);
            long fileSize = eos || stdInMode ? -1 : objInStream.Length;
            await objOutStream.WriteAsync(BitConverter.GetBytes(fileSize), 0, 8, token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            IAsyncCodeProgress funcProgress = funcOnProgress != null ? new AsyncDelegateCodeProgress(funcOnProgress) : null;
            await Task.Run(() => encoder.CodeAsync(objInStream, objOutStream, -1, -1, funcProgress, token), token).ConfigureAwait(false);
        }

        public static void DecompressLzmaFile(this FileStream objInStream, Stream objOutStream, Action<long, long> funcOnProgress = null)
        {
            Decoder decoder = s_LzyDecoder.Value;

            byte[] properties = new byte[5];
            if (objInStream.Read(properties, 0, 5) != 5)
                throw new ArgumentException("input .lzma is too short");
            decoder.SetDecoderProperties(properties);
            byte[] achrBuffer = new byte[8];
            _ = objInStream.Read(achrBuffer, 0, 8);
            long outSize = BitConverter.ToInt64(achrBuffer, 0);
            long compressedSize = objInStream.Length - objInStream.Position;
            ICodeProgress funcProgress = funcOnProgress != null ? new DelegateCodeProgress(funcOnProgress) : null;
            decoder.Code(objInStream, objOutStream, compressedSize, outSize, funcProgress);
        }

        public static async Task DecompressLzmaFileAsync(this FileStream objInStream, Stream objOutStream, Func<long, long, Task> funcOnProgress = null,
                                                         CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Decoder decoder = s_LzyDecoder.Value;
            token.ThrowIfCancellationRequested();
            byte[] properties = new byte[5];
            if (await objInStream.ReadAsync(properties, 0, 5, token).ConfigureAwait(false) != 5)
                throw new ArgumentException("input .lzma is too short");
            decoder.SetDecoderProperties(properties);
            byte[] achrBuffer = new byte[8];
            _ = await objInStream.ReadAsync(achrBuffer, 0, 8, token).ConfigureAwait(false);
            long outSize = BitConverter.ToInt64(achrBuffer, 0);
            token.ThrowIfCancellationRequested();
            long compressedSize = objInStream.Length - objInStream.Position;
            token.ThrowIfCancellationRequested();
            IAsyncCodeProgress funcProgress = funcOnProgress != null ? new AsyncDelegateCodeProgress(funcOnProgress) : null;
            await Task.Run(() => decoder.CodeAsync(objInStream, objOutStream, compressedSize, outSize, funcProgress, token), token).ConfigureAwait(false);
        }

        private sealed class DelegateCodeProgress : ICodeProgress
        {
            private readonly Action<long, long> handler;
            public DelegateCodeProgress(Action<long, long> handler) => this.handler = handler;
            public void SetProgress(long inSize, long outSize) => handler(inSize, outSize);
        }

        private sealed class AsyncDelegateCodeProgress : IAsyncCodeProgress
        {
            private readonly Func<long, long, Task> handler;
            public AsyncDelegateCodeProgress(Func<long, long, Task> handler) => this.handler = handler;
            public Task SetProgressAsync(long inSize, long outSize, CancellationToken token = default) => handler(inSize, outSize);
        }
    }
}
