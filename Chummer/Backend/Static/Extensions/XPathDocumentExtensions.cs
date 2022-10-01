using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public static class XPathDocumentExtensions
    {
        /// <summary>
        /// Syntactic sugar for synchronously loading an XPathDocument from a file with standard encoding and XmlReader settings
        /// </summary>
        /// <param name="strFileName">The file to use.</param>
        /// <param name="blnSafe">Whether or not to check characters for validity while loading.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XPathDocument LoadStandardFromFile(string strFileName, bool blnSafe = true)
        {
            using (StreamReader objStreamReader = new StreamReader(strFileName, Encoding.UTF8, true))
            using (XmlReader objReader = XmlReader.Create(objStreamReader,
                blnSafe ? GlobalSettings.SafeXmlReaderSettings : GlobalSettings.UnSafeXmlReaderSettings))
                return new XPathDocument(objReader);
        }

        /// <summary>
        /// Syntactic sugar for asynchronously loading an XPathDocument from a file with standard encoding and XmlReader settings
        /// </summary>
        /// <param name="strFileName">The file to use.</param>
        /// <param name="blnSafe">Whether or not to check characters for validity while loading.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<XPathDocument> LoadStandardFromFileAsync(string strFileName, bool blnSafe = true, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                using (StreamReader objStreamReader = new StreamReader(strFileName, Encoding.UTF8, true))
                {
                    token.ThrowIfCancellationRequested();
                    using (XmlReader objReader = XmlReader.Create(objStreamReader,
                                                                  blnSafe
                                                                      ? GlobalSettings.SafeXmlReaderSettings
                                                                      : GlobalSettings.UnSafeXmlReaderSettings))
                    {
                        token.ThrowIfCancellationRequested();
                        return new XPathDocument(objReader);
                    }
                }
            }, token);
        }

        /// <summary>
        /// Syntactic sugar for synchronously loading an XPathDocument from an LZMA-compressed file with standard encoding and XmlReader settings
        /// </summary>
        /// <param name="strFileName">The file to use.</param>
        /// <param name="blnSafe">Whether or not to check characters for validity while loading.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XPathDocument LoadStandardFromLzmaCompressedFile(string strFileName, bool blnSafe = true)
        {
            using (FileStream objFileStream = new FileStream(strFileName, FileMode.Open))
            using (MemoryStream objMemoryStream = new MemoryStream((int)objFileStream.Length))
            {
                objFileStream.DecompressLzmaFile(objMemoryStream);
                objMemoryStream.Seek(0, SeekOrigin.Begin);
                using (StreamReader objStreamReader
                       = new StreamReader(objMemoryStream, Encoding.UTF8, true))
                using (XmlReader objReader = XmlReader.Create(objStreamReader,
                                                              blnSafe
                                                                  ? GlobalSettings.SafeXmlReaderSettings
                                                                  : GlobalSettings.UnSafeXmlReaderSettings))
                    return new XPathDocument(objReader);
            }
        }

        /// <summary>
        /// Syntactic sugar for asynchronously loading an XPathDocument from an LZMA-compressed file with standard encoding and XmlReader settings
        /// </summary>
        /// <param name="strFileName">The file to use.</param>
        /// <param name="blnSafe">Whether or not to check characters for validity while loading.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<XPathDocument> LoadStandardFromLzmaCompressedFileAsync(string strFileName, bool blnSafe = true, CancellationToken token = default)
        {
            return Task.Run(async () =>
            {
                using (FileStream objFileStream = new FileStream(strFileName, FileMode.Open))
                {
                    token.ThrowIfCancellationRequested();
                    using (MemoryStream objMemoryStream = new MemoryStream((int)objFileStream.Length))
                    {
                        await objFileStream.DecompressLzmaFileAsync(objMemoryStream, token).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        objMemoryStream.Seek(0, SeekOrigin.Begin);
                        token.ThrowIfCancellationRequested();
                        using (StreamReader objStreamReader
                               = new StreamReader(objMemoryStream, Encoding.UTF8, true))
                        {
                            token.ThrowIfCancellationRequested();
                            using (XmlReader objReader = XmlReader.Create(objStreamReader,
                                                                          blnSafe
                                                                              ? GlobalSettings.SafeXmlReaderSettings
                                                                              : GlobalSettings.UnSafeXmlReaderSettings))
                            {
                                token.ThrowIfCancellationRequested();
                                return new XPathDocument(objReader);
                            }
                        }
                    }
                }
            }, token);
        }
    }
}
