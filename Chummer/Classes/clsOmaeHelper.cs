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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.ServiceModel;
using System.Xml;
using Chummer.OmaeService;
using Chummer.TranslationService;
using System.IO.Packaging;
 using System.Text;

namespace Chummer
{
    public static class OmaeHelper
    {
        /// <summary>
        /// Set all of the BasicHttpBinding properties and configure the EndPoint. This is done to avoid the need for an app.config file to be shipped with the application.
        /// </summary>
        public static omaeSoapClient GetOmaeService()
        {
            BasicHttpBinding objBinding = new BasicHttpBinding
            {
                Name = "omaeSoap",
                CloseTimeout = TimeSpan.FromMinutes(1),
                OpenTimeout = TimeSpan.FromMinutes(1),
                ReceiveTimeout = TimeSpan.FromMinutes(10),
                SendTimeout = TimeSpan.FromMinutes(1),
                AllowCookies = false,
                BypassProxyOnLocal = false,
                HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
                MaxBufferSize = 5242880, // 5 MB
                MaxReceivedMessageSize = 5242880, // 5 MB
                MaxBufferPoolSize = 524288,
                MessageEncoding = WSMessageEncoding.Text,
                TextEncoding = Encoding.UTF8,
                TransferMode = TransferMode.Buffered,
                UseDefaultWebProxy = true,
                ReaderQuotas =
                {
                    MaxDepth = 32,
                    MaxStringContentLength = 8388608,
                    MaxArrayLength = 5242880,
                    MaxBytesPerRead = 4096,
                    MaxNameTableCharCount = 32565
                },
                Security =
                {
                    Mode = BasicHttpSecurityMode.None,
                    Transport = {ClientCredentialType = HttpClientCredentialType.None, ProxyCredentialType = HttpProxyCredentialType.None, Realm = string.Empty},
                    Message = {ClientCredentialType = BasicHttpMessageCredentialType.UserName, AlgorithmSuite = System.ServiceModel.Security.SecurityAlgorithmSuite.Default}
                }
            };



            const string strEndPoint = "http://www.chummergen.com/dev/chummer/omae/omae.asmx";
            EndpointAddress objEndPointAddress = new EndpointAddress(strEndPoint);

            omaeSoapClient objService = new omaeSoapClient(objBinding, objEndPointAddress);

            return objService;
        }

        /// <summary>
        /// Set all of the BasicHttpBinding properties and configure the EndPoint. This is done to avoid the need for an app.config file to be shipped with the application.
        /// </summary>
        public static translationSoapClient GetTranslationService()
        {
            BasicHttpBinding objBinding = new BasicHttpBinding
            {
                Name = "translationSoap",
                CloseTimeout = TimeSpan.FromMinutes(1),
                OpenTimeout = TimeSpan.FromMinutes(1),
                ReceiveTimeout = TimeSpan.FromMinutes(10),
                SendTimeout = TimeSpan.FromMinutes(1),
                AllowCookies = false,
                BypassProxyOnLocal = false,
                HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
                MaxBufferSize = 5242880, // 5 MB
                MaxReceivedMessageSize = 5242880, // 5 MB
                MaxBufferPoolSize = 524288,
                MessageEncoding = WSMessageEncoding.Text,
                TextEncoding = Encoding.UTF8,
                TransferMode = TransferMode.Buffered,
                UseDefaultWebProxy = true,
                ReaderQuotas =
                {
                    MaxDepth = 32,
                    MaxStringContentLength = 8388608,
                    MaxArrayLength = 5242880,
                    MaxBytesPerRead = 4096,
                    MaxNameTableCharCount = 32565
                },
                Security =
                {
                    Mode = BasicHttpSecurityMode.None,
                    Transport = {ClientCredentialType = HttpClientCredentialType.None, ProxyCredentialType = HttpProxyCredentialType.None, Realm = string.Empty},
                    Message = {ClientCredentialType = BasicHttpMessageCredentialType.UserName, AlgorithmSuite = System.ServiceModel.Security.SecurityAlgorithmSuite.Default}
                }
            };



            const string strEndPoint = "http://www.chummergen.com/dev/chummer/omae/translation.asmx";
            EndpointAddress objEndPointAddress = new EndpointAddress(strEndPoint);

            translationSoapClient objService = new translationSoapClient(objBinding, objEndPointAddress);

            return objService;
        }

        /// <summary>
        /// Write the contents of a MemoryStream to an XmlDocument.
        /// </summary>
        /// <param name="objStream">MemoryStream to read.</param>
        public static XmlDocument XmlDocumentFromStream(MemoryStream objStream)
        {
            if (objStream == null)
                return null;
            objStream.Position = 0;
            XmlDocument objXmlDocument = new XmlDocument
            {
                XmlResolver = null
            };
            using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                using (XmlReader objXmlReader = XmlReader.Create(objReader, new XmlReaderSettings {XmlResolver = null}))
                    objXmlDocument.Load(objXmlReader);
            return objXmlDocument;
        }

        #region Base64
        /// <summary>
        /// Base64 encode a string.
        /// </summary>
        public static string Base64Encode(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
            }

            return null;
        }

        /// <summary>
        /// Decode a Base64 encoded string.
        /// </summary>
        public static string Base64Decode(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                UTF8Encoding encoder = new UTF8Encoding();
                Decoder utf8Decode = encoder.GetDecoder();

                byte[] bytToDecode = Convert.FromBase64String(data);
                int charCount = utf8Decode.GetCharCount(bytToDecode, 0, bytToDecode.Length);
                char[] chrDecoded = new char[charCount];
                utf8Decode.GetChars(bytToDecode, 0, bytToDecode.Length, chrDecoded, 0);
                return new string(chrDecoded);
            }

            return null;
        }
        #endregion

        #region Compression
        /// <summary>
        /// Compresses byte array to new byte array.
        /// </summary>
        public static byte[] Compress(byte[] raw)
        {
            if (raw == null)
                throw new ArgumentNullException(nameof(raw));
            using (MemoryStream memory = new MemoryStream())
            {
                // gzip.Dispose() should call memory.Dispose()
                using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true))
                    gzip.Write(raw, 0, raw.Length);
                return memory.ToArray();
            }
        }

        /// <summary>
        /// Decompress byte array to a new byte array.
        /// </summary>
        public static byte[] Decompress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            const int size = 4096;
            byte[] buffer = new byte[size];
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
                {
                    int count;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    } while (count > 0);
                }

                return memory.ToArray();
            }
        }

        /// <summary>
        /// Compress multiple files to a byte array.
        /// </summary>
        /// <param name="lstFiles">List of files to compress.</param>
        public static byte[] CompressMutiple(IEnumerable<string> lstFiles)
        {
            if (lstFiles == null)
                throw new ArgumentNullException(nameof(lstFiles));
            using (MemoryStream objStream = new MemoryStream())
            {
                using (Package objPackage = Package.Open(objStream, FileMode.Create, FileAccess.ReadWrite))
                {
                    foreach (string strFile in lstFiles)
                    {
                        Uri objUri = new Uri("/" + (Path.GetFileName(strFile)?.Replace(' ', '_') ?? string.Empty), UriKind.Relative);
                        PackagePart objPart = objPackage.CreatePart(objUri, System.Net.Mime.MediaTypeNames.Application.Zip, CompressionOption.Maximum);
                        byte[] bytBuffer = File.ReadAllBytes(strFile);
                        objPart?.GetStream().Write(bytBuffer, 0, bytBuffer.Length);
                    }
                }

                return objStream.ToArray();
            }
        }

        /// <summary>
        /// Compress multiple files to a file. This is only used for testing an compressing NPC packs. This is not to be used by end users.
        /// </summary>
        /// <param name="lstFiles">List of files to compress.</param>
        /// <param name="strDestination">File to compress to.</param>
        public static void CompressMutipleToFile(IEnumerable<string> lstFiles, string strDestination)
        {
            if (lstFiles == null)
                throw  new ArgumentNullException(nameof(lstFiles));
            using (Package objPackage = Package.Open(strDestination, FileMode.Create, FileAccess.ReadWrite))
            {
                foreach (string strFile in lstFiles)
                {
                    string[] strPath = Path.GetDirectoryName(strFile)?.Replace(' ', '_').Split(Path.DirectorySeparatorChar) ?? new string[] { };
                    string strPackFile = '/' + strPath[strPath.Length - 2] + '/' + strPath[strPath.Length - 1] + '/' + (Path.GetFileName(strFile)?.Replace(' ', '_') ?? string.Empty);
                    strPackFile = strPackFile.TrimStartOnce("/saves");
                    Uri objUri = new Uri(strPackFile, UriKind.Relative);
                    PackagePart objPart = objPackage.CreatePart(objUri, System.Net.Mime.MediaTypeNames.Application.Zip, CompressionOption.Maximum);
                    byte[] bytBuffer = File.ReadAllBytes(strFile);
                    objPart?.GetStream().Write(bytBuffer, 0, bytBuffer.Length);
                }
            }
        }

        /// <summary>
        /// Decompress multiple files from a single zip file.
        /// </summary>
        /// <param name="bytBuffer">Byte array that contains the zip file.</param>
        /// <param name="strPrefix">Prefix to attach to the decompressed files.</param>
        public static void DecompressDataFile(byte[] bytBuffer, string strPrefix)
        {
            if (bytBuffer == null)
                return;
            string strFilePath = Path.Combine(Utils.GetStartupPath, "data");
            using (MemoryStream objStream = new MemoryStream())
            {
                objStream.Write(bytBuffer, 0, bytBuffer.Length);
                using (Package objPackage = Package.Open(objStream, FileMode.Open, FileAccess.Read))
                {
                    foreach (PackagePart objPart in objPackage.GetParts())
                    {
                        string strTarget = Path.Combine(strFilePath, objPart.Uri.ToString().FastEscape('/'))
                            .Replace("\\override", Path.DirectorySeparatorChar + "override" + strPrefix)
                            .Replace("\\custom", Path.DirectorySeparatorChar + "custom" + strPrefix);

                        byte[] bytFileBuffer = new byte[100000];
                        using (Stream objSource = objPart.GetStream(FileMode.Open, FileAccess.Read))
                        {
                            using (Stream objDestination = File.OpenWrite(strTarget))
                            {
                                int intRead = objSource.Read(bytFileBuffer, 0, bytFileBuffer.Length);
                                while (intRead > 0)
                                {
                                    objDestination.Write(bytFileBuffer, 0, intRead);
                                    intRead = objSource.Read(bytFileBuffer, 0, bytFileBuffer.Length);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Decompress multiple files from a single zip file.
        /// </summary>
        /// /// <param name="bytBuffer">Byte array that contains the zip file.</param>
        public static void DecompressCharacterSheet(byte[] bytBuffer)
        {
            if (bytBuffer == null)
                return;
            string strFilePath = Path.Combine(Utils.GetStartupPath, "sheets", "omae");
            using (MemoryStream objStream = new MemoryStream())
            {
                objStream.Write(bytBuffer, 0, bytBuffer.Length);
                using (Package objPackage = Package.Open(objStream, FileMode.Open, FileAccess.Read))
                {
                    foreach (PackagePart objPart in objPackage.GetParts())
                    {
                        string strTarget = Path.Combine(strFilePath, objPart.Uri.ToString().FastEscape('/').Replace('_', ' '));

                        byte[] bytFileBuffer = new byte[100000];
                        using (Stream objSource = objPart.GetStream(FileMode.Open, FileAccess.Read))
                        {
                            using (Stream objDestination = File.OpenWrite(strTarget))
                            {
                                int intRead = objSource.Read(bytFileBuffer, 0, bytFileBuffer.Length);
                                while (intRead > 0)
                                {
                                    objDestination.Write(bytFileBuffer, 0, intRead);
                                    intRead = objSource.Read(bytFileBuffer, 0, bytFileBuffer.Length);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Decompress multiple files from a single zip file.
        /// </summary>
        /// <param name="bytBuffer">Byte array that contains the zip file.</param>
        public static void DecompressNPCs(byte[] bytBuffer)
        {
            if (bytBuffer == null)
                throw new ArgumentNullException(nameof(bytBuffer));
            string strFilePath = Path.Combine(Utils.GetStartupPath, "saves");

            // If the directory does not exist, create it.
            if (!Directory.Exists(strFilePath))
                Directory.CreateDirectory(strFilePath);

            using (MemoryStream objStream = new MemoryStream())
            {
                objStream.Write(bytBuffer, 0, bytBuffer.Length);
                using (Package objPackage = Package.Open(objStream, FileMode.Open, FileAccess.Read))
                {
                    foreach (PackagePart objPart in objPackage.GetParts())
                    {
                        string strTarget = Path.Combine(strFilePath, objPart.Uri.ToString().Replace('_', ' '));

                        string[] strDirectory = strTarget.Split('/');
                        if (!strDirectory[1].EndsWith(".chum5", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!Directory.Exists(Path.Combine(strFilePath, strDirectory[1])))
                                Directory.CreateDirectory(Path.Combine(strFilePath, strDirectory[1]));
                        }

                        if (!strDirectory[2].EndsWith(".chum5", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!Directory.Exists(Path.Combine(strFilePath, strDirectory[1] + Path.DirectorySeparatorChar + strDirectory[2])))
                                Directory.CreateDirectory(Path.Combine(strFilePath, strDirectory[1] + Path.DirectorySeparatorChar + strDirectory[2]));
                        }

                        byte[] bytFileBuffer = new byte[100000];
                        using (Stream objDestination = File.OpenWrite(strFilePath + strTarget.Replace('/', Path.DirectorySeparatorChar)))
                        {
                            using (Stream objSource = objPart.GetStream(FileMode.Open, FileAccess.Read))
                            {
                                int intRead = objSource.Read(bytFileBuffer, 0, bytFileBuffer.Length);
                                while (intRead > 0)
                                {
                                    objDestination.Write(bytFileBuffer, 0, intRead);
                                    intRead = objSource.Read(bytFileBuffer, 0, bytFileBuffer.Length);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Decompress multiple files from a single zip file.
        /// </summary>
        /// <param name="strExtract">Zip file to extract from.</param>
        public static void DecompressNPCs(string strExtract)
        {
            string strFilePath = Path.Combine(Utils.GetStartupPath, "saves");

            // If the directory does not exist, create it.
            if (!Directory.Exists(strFilePath))
                Directory.CreateDirectory(strFilePath);

            using (Package objPackage = Package.Open(strExtract, FileMode.Open, FileAccess.Read))
            {
                foreach (PackagePart objPart in objPackage.GetParts())
                {
                    string strTarget = Path.Combine(strFilePath, objPart.Uri.ToString().Replace('_', ' '));

                    string[] strDirectory = strTarget.Split('/');
                    if (!strDirectory[1].EndsWith(".chum5", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!Directory.Exists(Path.Combine(strFilePath, strDirectory[1])))
                            Directory.CreateDirectory(Path.Combine(strFilePath, strDirectory[1]));
                    }

                    if (!strDirectory[2].EndsWith(".chum5", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!Directory.Exists(Path.Combine(strFilePath, strDirectory[1] + Path.DirectorySeparatorChar + strDirectory[2])))
                            Directory.CreateDirectory(Path.Combine(strFilePath, strDirectory[1] + Path.DirectorySeparatorChar + strDirectory[2]));
                    }
                    byte[] bytFileBuffer = new byte[100000];
                    using (Stream objSource = objPart.GetStream(FileMode.Open, FileAccess.Read))
                    {
                        using (Stream objDestination = File.OpenWrite(strFilePath + strTarget.Replace('/', Path.DirectorySeparatorChar)))
                        {
                            int intRead = objSource.Read(bytFileBuffer, 0, bytFileBuffer.Length);
                            while (intRead > 0)
                            {
                                objDestination.Write(bytFileBuffer, 0, intRead);
                                intRead = objSource.Read(bytFileBuffer, 0, bytFileBuffer.Length);
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
