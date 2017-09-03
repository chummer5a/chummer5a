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
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.ServiceModel;
using System.Windows.Forms;
using System.Xml;
using Chummer.OmaeService;
using Chummer.TranslationService;
using System.IO.Packaging;

namespace Chummer
{
    class OmaeHelper
    {
        /// <summary>
        /// Set all of the BasicHttpBinding properties and configure the EndPoint. This is done to avoid the need for an app.config file to be shippped with the application.
        /// </summary>
        public omaeSoapClient GetOmaeService()
        {
            BasicHttpBinding objBinding = new BasicHttpBinding();
            objBinding.Name = "omaeSoap";
            objBinding.CloseTimeout = TimeSpan.FromMinutes(1);
            objBinding.OpenTimeout = TimeSpan.FromMinutes(1);
            objBinding.ReceiveTimeout = TimeSpan.FromMinutes(10);
            objBinding.SendTimeout = TimeSpan.FromMinutes(1);
            objBinding.AllowCookies = false;
            objBinding.BypassProxyOnLocal = false;
            objBinding.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;
            objBinding.MaxBufferSize = 5242880; // 5 MB
            objBinding.MaxReceivedMessageSize = 5242880; // 5 MB
            objBinding.MaxBufferPoolSize = 524288;
            objBinding.MessageEncoding = WSMessageEncoding.Text;
            objBinding.TextEncoding = System.Text.Encoding.UTF8;
            objBinding.TransferMode = TransferMode.Buffered;
            objBinding.UseDefaultWebProxy = true;

            objBinding.ReaderQuotas.MaxDepth = 32;
            objBinding.ReaderQuotas.MaxStringContentLength = 8388608;
            objBinding.ReaderQuotas.MaxArrayLength = 5242880;
            objBinding.ReaderQuotas.MaxBytesPerRead = 4096;
            objBinding.ReaderQuotas.MaxNameTableCharCount = 32565;

            objBinding.Security.Mode = BasicHttpSecurityMode.None;
            objBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            objBinding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
            objBinding.Security.Transport.Realm = string.Empty;
            objBinding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
            objBinding.Security.Message.AlgorithmSuite = System.ServiceModel.Security.SecurityAlgorithmSuite.Default;

            const string strEndPoint = "http://www.chummergen.com/dev/chummer/omae/omae.asmx";
            EndpointAddress objEndPointAddress = new EndpointAddress(strEndPoint);

            omaeSoapClient objService = new omaeSoapClient(objBinding, objEndPointAddress);

            return objService;
        }

        /// <summary>
        /// Set all of the BasicHttpBinding properties and configure the EndPoint. This is done to avoid the need for an app.config file to be shippped with the application.
        /// </summary>
        public translationSoapClient GetTranslationService()
        {
            BasicHttpBinding objBinding = new BasicHttpBinding();
            objBinding.Name = "translationSoap";
            objBinding.CloseTimeout = TimeSpan.FromMinutes(1);
            objBinding.OpenTimeout = TimeSpan.FromMinutes(1);
            objBinding.ReceiveTimeout = TimeSpan.FromMinutes(10);
            objBinding.SendTimeout = TimeSpan.FromMinutes(1);
            objBinding.AllowCookies = false;
            objBinding.BypassProxyOnLocal = false;
            objBinding.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;
            objBinding.MaxBufferSize = 5242880; // 5 MB
            objBinding.MaxReceivedMessageSize = 5242880; // 5 MB
            objBinding.MaxBufferPoolSize = 524288;
            objBinding.MessageEncoding = WSMessageEncoding.Text;
            objBinding.TextEncoding = System.Text.Encoding.UTF8;
            objBinding.TransferMode = TransferMode.Buffered;
            objBinding.UseDefaultWebProxy = true;

            objBinding.ReaderQuotas.MaxDepth = 32;
            objBinding.ReaderQuotas.MaxStringContentLength = 8388608;
            objBinding.ReaderQuotas.MaxArrayLength = 5242880;
            objBinding.ReaderQuotas.MaxBytesPerRead = 4096;
            objBinding.ReaderQuotas.MaxNameTableCharCount = 32565;

            objBinding.Security.Mode = BasicHttpSecurityMode.None;
            objBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            objBinding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
            objBinding.Security.Transport.Realm = string.Empty;
            objBinding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
            objBinding.Security.Message.AlgorithmSuite = System.ServiceModel.Security.SecurityAlgorithmSuite.Default;

            const string strEndPoint = "http://www.chummergen.com/dev/chummer/omae/translation.asmx";
            EndpointAddress objEndPointAddress = new EndpointAddress(strEndPoint);

            translationSoapClient objService = new translationSoapClient(objBinding, objEndPointAddress);

            return objService;
        }

        /// <summary>
        /// Write the contents of a MemoryStream to an XmlDocument.
        /// </summary>
        /// <param name="objStream">MemoryStream to read.</param>
        public XmlDocument XmlDocumentFromStream(MemoryStream objStream)
        {
            string strXml = string.Empty;
            objStream.Position = 0;
            StreamReader objReader = new StreamReader(objStream);
            strXml = objReader.ReadToEnd();

            XmlDocument objXmlDocument = new XmlDocument();
            objXmlDocument.LoadXml(strXml);

            return objXmlDocument;
        }

        #region Base64
        /// <summary>
        /// Base64 encode a string.
        /// </summary>
        public string Base64Encode(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                byte[] bytEncode = new byte[data.Length];
                bytEncode = System.Text.Encoding.UTF8.GetBytes(data);
                return Convert.ToBase64String(bytEncode);
            }

            return null;
        }

        /// <summary>
        /// Decode a Base64 encoded string.
        /// </summary>
        public string Base64Decode(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
                System.Text.Decoder utf8Decode = encoder.GetDecoder();

                byte[] bytToDecode = Convert.FromBase64String(data);
                int charCount = utf8Decode.GetCharCount(bytToDecode, 0, bytToDecode.Length);
                char[] chrDecoded = new char[charCount];
                utf8Decode.GetChars(bytToDecode, 0, bytToDecode.Length, chrDecoded, 0);
                return new String(chrDecoded);
            }

            return null;
        }
        #endregion

        #region Compression
        /// <summary>
        /// Compresses byte array to new byte array.
        /// </summary>
        public byte[] Compress(byte[] raw)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }

        /// <summary>
        /// Decompress byte array to a new byte array.
        /// </summary>
        public byte[] Decompress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }

        /// <summary>
        /// Compress multiple files to a byte array.
        /// </summary>
        /// <param name="lstFiles">List of files to compress.</param>
        public byte[] CompressMutiple(List<string> lstFiles)
        {
            MemoryStream objStream = new MemoryStream();
            Package objPackage = Package.Open(objStream, FileMode.Create, FileAccess.ReadWrite);

            foreach (string strFile in lstFiles)
            {
                Uri objUri = new Uri("/" + Path.GetFileName(strFile).Replace(' ', '_'), UriKind.Relative);
                PackagePart objPart = objPackage.CreatePart(objUri, System.Net.Mime.MediaTypeNames.Application.Zip, CompressionOption.Maximum);
                byte[] bytBuffer = File.ReadAllBytes(strFile);
                objPart.GetStream().Write(bytBuffer, 0, bytBuffer.Length);
            }
            objPackage.Close();

            return objStream.ToArray();
        }

        /// <summary>
        /// Compress multiple files to a file. This is only used for testing an compressing NPC packs. This is not to be used by end users.
        /// </summary>
        /// <param name="lstFiles">List of files to compress.</param>
        /// <param name="strDestination">File to compress to.</param>
        public void CompressMutipleToFile(List<string> lstFiles, string strDestination)
        {
            Package objPackage = Package.Open(strDestination, FileMode.Create, FileAccess.ReadWrite);

            foreach (string strFile in lstFiles)
            {
                string[] strPath = Path.GetDirectoryName(strFile).Replace(' ', '_').Split('\\');
                string strPackFile = "/" + strPath[strPath.Length - 2] + "/" + strPath[strPath.Length - 1] + "/" + Path.GetFileName(strFile).Replace(' ', '_');
                if (strPackFile.StartsWith("/saves"))
                    strPackFile = strPackFile.Replace("/saves", string.Empty);
                Uri objUri = new Uri(strPackFile, UriKind.Relative);
                PackagePart objPart = objPackage.CreatePart(objUri, System.Net.Mime.MediaTypeNames.Application.Zip, CompressionOption.Maximum);
                byte[] bytBuffer = File.ReadAllBytes(strFile);
                objPart.GetStream().Write(bytBuffer, 0, bytBuffer.Length);
            }
            objPackage.Close();
        }

        /// <summary>
        /// Decompress multiple files from a single zip file.
        /// </summary>
        /// <param name="bytBuffer">Byte array that contains the zip file.</param>
        /// <param name="strPrefix">Prefix to attach to the decompressed files.</param>
        public void DecompressDataFile(byte[] bytBuffer, string strPrefix)
        {
            string strFilePath = Path.Combine(Application.StartupPath, "data");
            MemoryStream objStream = new MemoryStream();
            objStream.Write(bytBuffer, 0, bytBuffer.Length);
            Package objPackage = Package.Open(objStream, FileMode.Open, FileAccess.Read);

            foreach (PackagePart objPart in objPackage.GetParts())
            {
                string strTarget = Path.Combine(strFilePath, objPart.Uri.ToString().Replace("/", string.Empty));
                strTarget = strTarget.Replace("\\override", Path.DirectorySeparatorChar + "override" + strPrefix);
                strTarget = strTarget.Replace("\\custom", Path.DirectorySeparatorChar + "custom" + strPrefix);

                Stream objSource = objPart.GetStream(FileMode.Open, FileAccess.Read);
                Stream objDestination = File.OpenWrite(strTarget);
                byte[] bytFileBuffer = new byte[100000];
                int intRead;
                intRead = objSource.Read(bytFileBuffer, 0, bytFileBuffer.Length);
                while (intRead > 0)
                {
                    objDestination.Write(bytFileBuffer, 0, intRead);
                    intRead = objSource.Read(bytFileBuffer, 0, bytFileBuffer.Length);
                }
                objDestination.Close();
                objSource.Close();
            }
        }

        /// <summary>
        /// Decompress multiple files from a single zip file.
        /// </summary>
        /// /// <param name="bytBuffer">Byte array that contains the zip file.</param>
        public void DecompressCharacterSheet(byte[] bytBuffer)
        {
            string strFilePath = Path.Combine(Application.StartupPath, "sheets", "omae");
            MemoryStream objStream = new MemoryStream();
            objStream.Write(bytBuffer, 0, bytBuffer.Length);
            Package objPackage = Package.Open(objStream, FileMode.Open, FileAccess.Read);

            foreach (PackagePart objPart in objPackage.GetParts())
            {
                string strTarget = Path.Combine(strFilePath, objPart.Uri.ToString().Replace("/", string.Empty).Replace('_', ' '));

                Stream objSource = objPart.GetStream(FileMode.Open, FileAccess.Read);
                Stream objDestination = File.OpenWrite(strTarget);
                byte[] bytFileBuffer = new byte[100000];
                int intRead;
                intRead = objSource.Read(bytFileBuffer, 0, bytFileBuffer.Length);
                while (intRead > 0)
                {
                    objDestination.Write(bytFileBuffer, 0, intRead);
                    intRead = objSource.Read(bytFileBuffer, 0, bytFileBuffer.Length);
                }
                objDestination.Close();
                objSource.Close();
            }
        }

        /// <summary>
        /// Decompress multiple files from a single zip file.
        /// </summary>
        /// <param name="bytBuffer">Byte array that contains the zip file.</param>
        public void DecompressNPCs(byte[] bytBuffer)
        {
            string strFilePath = Path.Combine(Application.StartupPath, "saves");

            // If the directory does not exist, create it.
            if (!Directory.Exists(strFilePath))
                Directory.CreateDirectory(strFilePath);

            MemoryStream objStream = new MemoryStream();
            objStream.Write(bytBuffer, 0, bytBuffer.Length);
            Package objPackage = Package.Open(objStream, FileMode.Open, FileAccess.Read);

            foreach (PackagePart objPart in objPackage.GetParts())
            {
                string strTarget = Path.Combine(strFilePath, objPart.Uri.ToString().Replace('_', ' '));

                Stream objSource = objPart.GetStream(FileMode.Open, FileAccess.Read);

                string[] strDirectory = strTarget.Split('/');
                if (!strDirectory[1].EndsWith(".chum5"))
                {
                    if (!Directory.Exists(Path.Combine(strFilePath, strDirectory[1])))
                        Directory.CreateDirectory(Path.Combine(strFilePath, strDirectory[1]));
                }
                if (!strDirectory[2].EndsWith(".chum5"))
                {
                    if (!Directory.Exists(Path.Combine(strFilePath, strDirectory[1] + Path.DirectorySeparatorChar + strDirectory[2])))
                        Directory.CreateDirectory(Path.Combine(strFilePath, strDirectory[1] + Path.DirectorySeparatorChar + strDirectory[2]));
                }

                Stream objDestination = File.OpenWrite(strFilePath + strTarget.Replace('/', Path.DirectorySeparatorChar));
                byte[] bytFileBuffer = new byte[100000];
                int intRead;
                intRead = objSource.Read(bytFileBuffer, 0, bytFileBuffer.Length);
                while (intRead > 0)
                {
                    objDestination.Write(bytFileBuffer, 0, intRead);
                    intRead = objSource.Read(bytFileBuffer, 0, bytFileBuffer.Length);
                }
                objDestination.Close();
                objSource.Close();
            }
        }

        /// <summary>
        /// Decompress multiple files from a single zip file.
        /// </summary>
        /// <param name="strExtract">Zip file to extract from.</param>
        public void DecompressNPCs(string strExtract)
        {
            string strFilePath = Path.Combine(Application.StartupPath, "saves");

            // If the directory does not exist, create it.
            if (!Directory.Exists(strFilePath))
                Directory.CreateDirectory(strFilePath);

            Package objPackage = Package.Open(strExtract, FileMode.Open, FileAccess.Read);

            foreach (PackagePart objPart in objPackage.GetParts())
            {
                string strTarget = Path.Combine(strFilePath, objPart.Uri.ToString().Replace('_', ' '));

                Stream objSource = objPart.GetStream(FileMode.Open, FileAccess.Read);

                string[] strDirectory = strTarget.Split('/');
                if (!strDirectory[1].EndsWith(".chum5"))
                {
                    if (!Directory.Exists(Path.Combine(strFilePath, strDirectory[1])))
                        Directory.CreateDirectory(Path.Combine(strFilePath, strDirectory[1]));
                }
                if (!strDirectory[2].EndsWith(".chum5"))
                {
                    if (!Directory.Exists(Path.Combine(strFilePath, strDirectory[1] + Path.DirectorySeparatorChar + strDirectory[2])))
                        Directory.CreateDirectory(Path.Combine(strFilePath, strDirectory[1] + Path.DirectorySeparatorChar + strDirectory[2]));
                }

                Stream objDestination = File.OpenWrite(strFilePath + strTarget.Replace('/', Path.DirectorySeparatorChar));
                byte[] bytFileBuffer = new byte[100000];
                int intRead;
                intRead = objSource.Read(bytFileBuffer, 0, bytFileBuffer.Length);
                while (intRead > 0)
                {
                    objDestination.Write(bytFileBuffer, 0, intRead);
                    intRead = objSource.Read(bytFileBuffer, 0, bytFileBuffer.Length);
                }
                objDestination.Close();
                objSource.Close();
            }
        }
        #endregion
    }
}