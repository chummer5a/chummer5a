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
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Web;
using System.Threading;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Serialization;

namespace Codaxy.WkHtmlToPdf
{
    [Serializable]
    public class PdfConvertException : Exception
    {
        public PdfConvertException() : base() { }
        public PdfConvertException(string msg) : base(msg) { }
        public PdfConvertException(string msg, Exception innerException) : base(msg, innerException) { }
        protected PdfConvertException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class PdfConvertTimeoutException : PdfConvertException
    {
        public PdfConvertTimeoutException() : base("HTML to PDF conversion process has not finished in the given period.") { }
        public PdfConvertTimeoutException(string msg) : base("HTML to PDF conversion process has not finished in the given period.") { }
        public PdfConvertTimeoutException(string msg, Exception innerException) : base("HTML to PDF conversion process has not finished in the given period.") { }
        protected PdfConvertTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    public class PdfOutput
    {
        public string OutputFilePath { get; set; }
        public Stream OutputStream { get; set; }
        public Action<PdfDocument, byte[]> OutputCallback { get; set; }
    }

    public class PdfDocument
    {
        public string Url { get; set; }
        public string Html { get; set; }
        public string HeaderUrl { get; set; }
        public string FooterUrl { get; set; }
        public string HeaderLeft { get; set; }
        public string HeaderCenter { get; set; }
        public string HeaderRight { get; set; }
        public string FooterLeft { get; set; }
        public string FooterCenter { get; set; }
        public string FooterRight { get; set; }
        public object State { get; set; }
        public IDictionary<string, string> Cookies { get; } = new Dictionary<string, string>();
        public IDictionary<string, string> ExtraParams { get; } = new Dictionary<string, string>();
        public string HeaderFontSize { get; set; }
        public string FooterFontSize { get; set; }
        public string HeaderFontName { get; set; }
        public string FooterFontName { get; set; }
    }

    public class PdfConvertEnvironment
    {
        public string TempFolderPath { get; set; }
        public string WkHtmlToPdfPath { get; set; }
        public int Timeout { get; set; }
        public bool Debug { get; set; }
    }

    public static class PdfConvert
    {
        static PdfConvertEnvironment s_E;

        public static PdfConvertEnvironment Environment => s_E ?? (s_E = new PdfConvertEnvironment
        {
            TempFolderPath = Path.GetTempPath(),
            WkHtmlToPdfPath = GetWkhtmlToPdfExeLocation(),
            Timeout = 60000
        });

        private static string GetWkhtmlToPdfExeLocation()
        {
            string filePath, customPath = ConfigurationManager.AppSettings["wkhtmltopdf:path"];

            if (customPath != null)
            {
                filePath = Path.Combine(customPath, @"wkhtmltopdf.exe");

                if (File.Exists(filePath))
                    return filePath;
            }

            string programFilesPath = System.Environment.GetEnvironmentVariable("ProgramFiles") ?? string.Empty;
            filePath = Path.Combine(programFilesPath, @"wkhtmltopdf\wkhtmltopdf.exe");

            if (File.Exists(filePath))
                return filePath;

            string programFilesx86Path = System.Environment.GetEnvironmentVariable("ProgramFiles(x86)") ?? string.Empty;
            filePath = Path.Combine(programFilesx86Path, @"wkhtmltopdf\wkhtmltopdf.exe");

            if (File.Exists(filePath))
                return filePath;

            filePath = Path.Combine(programFilesPath, @"wkhtmltopdf\bin\wkhtmltopdf.exe");
            if (File.Exists(filePath))
                return filePath;

            return Path.Combine(programFilesx86Path, @"wkhtmltopdf\bin\wkhtmltopdf.exe");
        }

        public static void ConvertHtmlToPdf(PdfDocument document, PdfOutput output)
        {
            ConvertHtmlToPdf(document, null, output);
        }

        public static void ConvertHtmlToPdf(PdfDocument document, PdfConvertEnvironment environment, PdfOutput woutput)
        {
            if (environment == null)
                environment = Environment;

            if (document.Html != null)
                document.Url = "-";

            string strOutputPdfFilePath;
            bool blnDelete;
            if (woutput.OutputFilePath != null)
            {
                strOutputPdfFilePath = woutput.OutputFilePath;
                blnDelete = false;
            }
            else
            {
                strOutputPdfFilePath = Path.Combine(environment.TempFolderPath, $"{Guid.NewGuid()}.pdf");
                blnDelete = true;
            }

            if (!File.Exists(environment.WkHtmlToPdfPath))
                throw new PdfConvertException($"File '{environment.WkHtmlToPdfPath}' not found. Check if wkhtmltopdf application is installed.");

            StringBuilder strbldParamsBuilder = new StringBuilder("--page-size A4 ");

            if (!string.IsNullOrEmpty(document.HeaderUrl))
            {
                strbldParamsBuilder.AppendFormat("--header-html {0} ", document.HeaderUrl);
                strbldParamsBuilder.Append("--margin-top 25 ");
                strbldParamsBuilder.Append("--header-spacing 5 ");
            }
            if (!string.IsNullOrEmpty(document.FooterUrl))
            {
                strbldParamsBuilder.AppendFormat("--footer-html {0} ", document.FooterUrl);
                strbldParamsBuilder.Append("--margin-bottom 25 ");
                strbldParamsBuilder.Append("--footer-spacing 5 ");
            }
            if (!string.IsNullOrEmpty(document.HeaderLeft))
                strbldParamsBuilder.AppendFormat("--header-left \"{0}\" ", document.HeaderLeft);

            if (!string.IsNullOrEmpty(document.HeaderCenter))
                strbldParamsBuilder.AppendFormat("--header-center \"{0}\" ", document.HeaderCenter);

            if (!string.IsNullOrEmpty(document.HeaderRight))
                strbldParamsBuilder.AppendFormat("--header-right \"{0}\" ", document.HeaderRight);

            if (!string.IsNullOrEmpty(document.FooterLeft))
                strbldParamsBuilder.AppendFormat("--footer-left \"{0}\" ", document.FooterLeft);

            if (!string.IsNullOrEmpty(document.FooterCenter))
                strbldParamsBuilder.AppendFormat("--footer-center \"{0}\" ", document.FooterCenter);

            if (!string.IsNullOrEmpty(document.FooterRight))
                strbldParamsBuilder.AppendFormat("--footer-right \"{0}\" ", document.FooterRight);

            if (!string.IsNullOrEmpty(document.HeaderFontSize))
                strbldParamsBuilder.AppendFormat("--header-font-size \"{0}\" ", document.HeaderFontSize);

            if (!string.IsNullOrEmpty(document.FooterFontSize))
                strbldParamsBuilder.AppendFormat("--footer-font-size \"{0}\" ", document.FooterFontSize);

            if (!string.IsNullOrEmpty(document.HeaderFontName))
                strbldParamsBuilder.AppendFormat("--header-font-name \"{0}\" ", document.HeaderFontName);

            if (!string.IsNullOrEmpty(document.FooterFontName))
                strbldParamsBuilder.AppendFormat("--footer-font-name \"{0}\" ", document.FooterFontName);


            foreach (var extraParam in document.ExtraParams)
                strbldParamsBuilder.AppendFormat("--{0} {1} ", extraParam.Key, extraParam.Value);

            foreach (var cookie in document.Cookies)
                strbldParamsBuilder.AppendFormat("--cookie {0} {1} ", cookie.Key, cookie.Value);

            strbldParamsBuilder.AppendFormat("\"{0}\" \"{1}\"", document.Url, strOutputPdfFilePath);

            try
            {
                StringBuilder strbldOutput = new StringBuilder();
                StringBuilder strbldError = new StringBuilder();

                using (Process process = new Process())
                {
                    process.StartInfo.FileName = environment.WkHtmlToPdfPath;
                    process.StartInfo.Arguments = strbldParamsBuilder.ToString();
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardInput = true;

                    AutoResetEvent errorWaitHandle = new AutoResetEvent(false);
                    AutoResetEvent outputWaitHandle = new AutoResetEvent(false);
                    try
                    {
                        void outputHandler(object sender, DataReceivedEventArgs e)
                        {
                            if (e.Data == null)
                            {
                                outputWaitHandle.Set();
                            }
                            else
                            {
                                strbldOutput.AppendLine(e.Data);
                            }
                        }

                        void errorHandler(object sender, DataReceivedEventArgs e)
                        {
                            if (e.Data == null)
                            {
                                errorWaitHandle.Set();
                            }
                            else
                            {
                                strbldError.AppendLine(e.Data);
                            }
                        }

                        process.OutputDataReceived += outputHandler;
                        process.ErrorDataReceived += errorHandler;

                        try
                        {
                            process.Start();

                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();

                            if (document.Html != null)
                            {
                                StreamWriter objStream = process.StandardInput;
                                byte[] buffer = Encoding.UTF8.GetBytes(document.Html);
                                objStream.BaseStream.Write(buffer, 0, buffer.Length);
                                objStream.WriteLine();
                                objStream.Flush();
                                objStream.Close();
                            }

                            if (process.WaitForExit(environment.Timeout) && outputWaitHandle.WaitOne(environment.Timeout) && errorWaitHandle.WaitOne(environment.Timeout))
                            {
                                if (process.ExitCode != 0 && !File.Exists(strOutputPdfFilePath))
                                {
                                    throw new PdfConvertException($"Html to PDF conversion of '{document.Url}' failed. Wkhtmltopdf output:{System.Environment.NewLine}{strbldError}");
                                }
                            }
                            else
                            {
                                if (!process.HasExited)
                                    process.Kill();

                                throw new PdfConvertTimeoutException();
                            }
                        }
                        finally
                        {
                            process.OutputDataReceived -= outputHandler;
                            process.ErrorDataReceived -= errorHandler;
                        }
                    }
                    finally
                    {
                        errorWaitHandle?.Close();
                        outputWaitHandle?.Close();
                    }
                }


                if (woutput.OutputStream != null)
                {
                    using (Stream fs = new FileStream(strOutputPdfFilePath, FileMode.Open))
                    {
                        byte[] buffer = new byte[32 * 1024];
                        int intRead;

                        while ((intRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                            woutput.OutputStream.Write(buffer, 0, intRead);
                    }
                }

                if (woutput.OutputCallback != null)
                {
                    byte[] pdfFileBytes = File.ReadAllBytes(strOutputPdfFilePath);
                    woutput.OutputCallback(document, pdfFileBytes);
                }

            }
            finally
            {
                if (blnDelete && File.Exists(strOutputPdfFilePath))
                    File.Delete(strOutputPdfFilePath);
            }
        }

        internal static void ConvertHtmlToPdf(string url, string outputFilePath)
        {
            ConvertHtmlToPdf(new PdfDocument { Url = url }, new PdfOutput { OutputFilePath = outputFilePath });
        }
    }

    //class OSUtil
    //{
    //    public static string GetProgramFilesx86Path()
    //    {
    //        if (8 == IntPtr.Size || (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
    //        {
    //            return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
    //        }
    //        return Environment.GetEnvironmentVariable("ProgramFiles");
    //    }
    //}

    //public static class HttpResponseExtensions
    //{
    //    public static void SendFileForDownload(this HttpResponse response, String filename, byte[] content)
    //    {
    //        SetFileDownloadHeaders(response, filename);
    //        response.OutputStream.Write(content, 0, content.Length);
    //        response.Flush();
    //    }

    //    public static void SendFileForDownload(this HttpResponse response, String filename)
    //    {
    //        SetFileDownloadHeaders(response, filename);
    //        response.TransmitFile(filename);
    //        response.Flush();
    //    }

    //    public static void SetFileDownloadHeaders(this HttpResponse response, String filename)
    //    {
    //        FileInfo fi = new FileInfo(filename);
    //        response.ContentType = "application/force-download";
    //        response.AddHeader("Content-Disposition", "attachment; filename=\"" + fi.Name + '\"');
    //    }
    //}
}
