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
                                              SevenZipCompressionLevel eSevenZipCompressionLevel)
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
                                              ChummerCompressionPreset eChummerCompressionPreset)
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
                                              Encoder.EMatchFinderType mf = Encoder.EMatchFinderType.BT4)
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
            for (int i = 0; i < 8; i++)
                objOutStream.WriteByte((byte)(fileSize >> (8 * i)));
            encoder.Code(objInStream, objOutStream, -1, -1, null);
        }

        public static Task CompressToLzmaFileAsync(this Stream objInStream, FileStream objOutStream,
                                                   SevenZipCompressionLevel eSevenZipCompressionLevel,
                                                   CancellationToken token = default)
        {
            // ReSharper disable RedundantArgumentDefaultValue
            switch (eSevenZipCompressionLevel)
            {
                case SevenZipCompressionLevel.Store:
                    return objInStream.CopyToAsync(objOutStream, 81920, token);

                case SevenZipCompressionLevel.Fastest:
                    return CompressToLzmaFileAsync(objInStream, objOutStream, 8, 32, Encoder.EMatchFinderType.BT2,
                                                   token);

                case SevenZipCompressionLevel.Fast:
                    return CompressToLzmaFileAsync(objInStream, objOutStream, 22, 32, Encoder.EMatchFinderType.BT2,
                                                   token);

                case SevenZipCompressionLevel.Normal:
                    return CompressToLzmaFileAsync(objInStream, objOutStream, 24, 32, Encoder.EMatchFinderType.BT4,
                                                   token);

                case SevenZipCompressionLevel.Maximum:
                    return CompressToLzmaFileAsync(objInStream, objOutStream, 25, 64, Encoder.EMatchFinderType.BT4,
                                                   token);

                case SevenZipCompressionLevel.Ultra:
                    return CompressToLzmaFileAsync(objInStream, objOutStream, 26, 64, Encoder.EMatchFinderType.BT4,
                                                   token);

                default:
                    goto case SevenZipCompressionLevel.Normal;
            }
            // ReSharper enable RedundantArgumentDefaultValue
        }

        public static Task CompressToLzmaFileAsync(this Stream objInStream, FileStream objOutStream,
                                                   ChummerCompressionPreset eChummerCompressionPreset,
                                                   CancellationToken token = default)
        {
            // ReSharper disable RedundantArgumentDefaultValue
            switch (eChummerCompressionPreset)
            {
                case ChummerCompressionPreset.Fast:
                    return CompressToLzmaFileAsync(objInStream, objOutStream, 22, 32, Encoder.EMatchFinderType.BT2,
                                                   token);

                case ChummerCompressionPreset.Balanced:
                    return CompressToLzmaFileAsync(objInStream, objOutStream, 24, 64, Encoder.EMatchFinderType.BT4,
                                                   token);

                case ChummerCompressionPreset.Thorough:
                    return CompressToLzmaFileAsync(objInStream, objOutStream, 26, 128, Encoder.EMatchFinderType.BT4,
                                                   token);

                default:
                    goto case ChummerCompressionPreset.Balanced;
            }
            // ReSharper restore RedundantArgumentDefaultValue
        }

        public static Task CompressToLzmaFileAsync(this Stream objInStream, FileStream objOutStream,
                                                   int intCompressionLevel = Encoder.kDefaultDictionaryLogSize,
                                                   int numFastBytes = (int)Encoder.kNumFastBytesDefault,
                                                   Encoder.EMatchFinderType mf = Encoder.EMatchFinderType.BT4,
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
            encoder.WriteCoderProperties(objOutStream);
            long fileSize = eos || stdInMode ? -1 : objInStream.Length;
            for (int i = 0; i < 8; i++)
            {
                token.ThrowIfCancellationRequested();
                objOutStream.WriteByte((byte)(fileSize >> (8 * i)));
            }

            token.ThrowIfCancellationRequested();
            return Task.Run(() => encoder.Code(objInStream, objOutStream, -1, -1, null, token), token);
        }

        public static void DecompressLzmaFile(this FileStream objInStream, Stream objOutStream)
        {
            Decoder decoder = s_LzyDecoder.Value;

            byte[] properties = new byte[5];
            if (objInStream.Read(properties, 0, 5) != 5)
                throw new Exception("input .lzma is too short");
            decoder.SetDecoderProperties(properties);

            long outSize = 0;
            for (int i = 0; i < 8; i++)
            {
                int v = objInStream.ReadByte();
                if (v < 0)
                    throw new Exception("Can't Read 1");
                outSize |= (long)(byte)v << (8 * i);
            }

            long compressedSize = objInStream.Length - objInStream.Position;

            decoder.Code(objInStream, objOutStream, compressedSize, outSize, null);
        }

        public static async Task DecompressLzmaFileAsync(this FileStream objInStream, Stream objOutStream,
                                                         CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Decoder decoder = s_LzyDecoder.Value;
            token.ThrowIfCancellationRequested();
            byte[] properties = new byte[5];
            if (await objInStream.ReadAsync(properties, 0, 5, token) != 5)
                throw new Exception("input .lzma is too short");
            decoder.SetDecoderProperties(properties);
            long outSize = 0;
            for (int i = 0; i < 8; i++)
            {
                token.ThrowIfCancellationRequested();
                int v = objInStream.ReadByte();
                if (v < 0)
                    throw new Exception("Can't Read 1");
                token.ThrowIfCancellationRequested();
                outSize |= (long)(byte)v << (8 * i);
            }

            long compressedSize = objInStream.Length - objInStream.Position;
            token.ThrowIfCancellationRequested();
            await Task.Run(() => decoder.Code(objInStream, objOutStream, compressedSize, outSize, null, token), token);
        }
    }
}
