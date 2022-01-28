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
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Chummer;

namespace Codaxy.WkHtmlToPdf
{
    public class PdfConvertException : Exception
    {
        public PdfConvertException(string msg) : base(msg)
        {
        }

        public PdfConvertException()
        {
        }

        public PdfConvertException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class PdfConvertTimeoutException : PdfConvertException
    {
        public PdfConvertTimeoutException() : base("HTML to PDF conversion process has not finished in the given period.")
        {
        }

        public PdfConvertTimeoutException(string msg) : base(msg)
        {
        }

        public PdfConvertTimeoutException(string message, Exception innerException) : base(message, innerException)
        {
        }
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
        public Dictionary<string, string> Cookies { get; set; }
        public Dictionary<string, string> ExtraParams { get; set; }
        public string HeaderFontSize { get; set; }
        public string FooterFontSize { get; set; }
        public string HeaderFontName { get; set; }
        public string FooterFontName { get; set; }
    }

    public class PdfConvertEnvironment
    {
        public string TempFolderPath { get; set; } = Path.GetTempPath();
        public string WkHtmlToPdfPath { get; set; }
        public int Timeout { get; set; } = 60000;
        public bool Debug { get; set; }
    }

    public static class PdfConvert
    {
        private static PdfConvertEnvironment _e;

        public static PdfConvertEnvironment Environment => _e ?? (_e = new PdfConvertEnvironment
            {WkHtmlToPdfPath = GetWkhtmlToPdfExeLocation()});

        private static string GetWkhtmlToPdfExeLocation()
        {
            string filePath, customPath = ConfigurationManager.AppSettings["wkhtmltopdf:path"];

            if (customPath != null)
            {
                filePath = Path.Combine(customPath, "wkhtmltopdf.exe");

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
            return File.Exists(filePath) ? filePath : Path.Combine(programFilesx86Path, @"wkhtmltopdf\bin\wkhtmltopdf.exe");
        }

        public static void ConvertHtmlToPdf(PdfDocument document, PdfOutput output)
        {
            ConvertHtmlToPdf(document, null, output);
        }

        public static void ConvertHtmlToPdf(PdfDocument document, PdfConvertEnvironment environment, PdfOutput woutput)
        {
            ConvertHtmlToPdfCoreAsync(true, document, environment, woutput).GetAwaiter().GetResult();
        }

        public static Task ConvertHtmlToPdfAsync(PdfDocument document, PdfConvertEnvironment environment, PdfOutput woutput)
        {
            return ConvertHtmlToPdfCoreAsync(false, document, environment, woutput);
        }

        private static async Task ConvertHtmlToPdfCoreAsync(bool blnSync, PdfDocument document, PdfConvertEnvironment environment, PdfOutput woutput)
        {
            if (environment == null)
                environment = Environment;

            if (document.Html != null)
                document.Url = "-";

            string outputPdfFilePath;
            bool delete;
            if (woutput.OutputFilePath != null)
            {
                outputPdfFilePath = woutput.OutputFilePath;
                delete = false;
            }
            else
            {
                outputPdfFilePath = Path.Combine(environment.TempFolderPath, Guid.NewGuid() + ".pdf");
                delete = true;
            }

            if (!File.Exists(environment.WkHtmlToPdfPath))
                throw new PdfConvertException($"File '{environment.WkHtmlToPdfPath}' not found. Check if wkhtmltopdf application is installed.");

            string strParams;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdParams))
            {
                sbdParams.Append("--page-size A4 ");
                sbdParams.Append("--disable-smart-shrinking ");

                if (!string.IsNullOrEmpty(document.HeaderUrl))
                {
                    sbdParams.Append("--header-html ").Append(document.HeaderUrl).Append(" --margin-top 25 ")
                             .Append("--margin-top 25 ").Append("--header-spacing 5 ");
                }

                if (!string.IsNullOrEmpty(document.FooterUrl))
                {
                    sbdParams.Append("--footer-html ").Append(document.FooterUrl).Append(" --margin-bottom 25 ")
                             .Append("--footer-spacing 5 ");
                }

                if (!string.IsNullOrEmpty(document.HeaderLeft))
                    sbdParams.Append("--header-left \"").Append(document.HeaderLeft).Append("\" ");

                if (!string.IsNullOrEmpty(document.HeaderCenter))
                    sbdParams.Append("--header-center \"").Append(document.HeaderCenter).Append("\" ");

                if (!string.IsNullOrEmpty(document.HeaderRight))
                    sbdParams.Append("--header-right \"").Append(document.HeaderRight).Append("\" ");

                if (!string.IsNullOrEmpty(document.FooterLeft))
                    sbdParams.Append("--footer-left \"").Append(document.FooterLeft).Append("\" ");

                if (!string.IsNullOrEmpty(document.FooterCenter))
                    sbdParams.Append("--footer-center \"").Append(document.FooterCenter).Append("\" ");

                if (!string.IsNullOrEmpty(document.FooterRight))
                    sbdParams.Append("--footer-right \"").Append(document.FooterRight).Append("\" ");

                if (!string.IsNullOrEmpty(document.HeaderFontSize))
                    sbdParams.Append("--header-font-size \"").Append(document.HeaderFontSize).Append("\" ");

                if (!string.IsNullOrEmpty(document.FooterFontSize))
                    sbdParams.Append("--footer-font-size \"").Append(document.FooterFontSize).Append("\" ");

                if (!string.IsNullOrEmpty(document.HeaderFontName))
                    sbdParams.Append("--header-font-name \"").Append(document.HeaderFontName).Append("\" ");

                if (!string.IsNullOrEmpty(document.FooterFontName))
                    sbdParams.Append("--footer-font-name \"").Append(document.FooterFontName).Append("\" ");

                if (document.ExtraParams != null)
                {
                    foreach (KeyValuePair<string, string> extraParam in document.ExtraParams)
                        sbdParams.Append("--").Append(extraParam.Key).Append(' ').Append(extraParam.Value)
                                 .Append(' ');
                }

                if (document.Cookies != null)
                {
                    foreach (KeyValuePair<string, string> cookie in document.Cookies)
                        sbdParams.Append("--cookie ").Append(cookie.Key).Append(' ').Append(cookie.Value)
                                 .Append(' ');
                }

                sbdParams.Append('\"').Append(document.Url).Append("\" \"").Append(outputPdfFilePath).Append('\"');
                strParams = sbdParams.ToString();
            }

            try
            {
                using (Process process = new Process { EnableRaisingEvents = true })
                {
                    process.StartInfo.FileName = environment.WkHtmlToPdfPath;
                    process.StartInfo.Arguments = strParams;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardInput = true;

                    using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                    using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder output))
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder error))
                    {
                        void OutputHandler(object sender, DataReceivedEventArgs e)
                        {
                            if (e.Data == null)
                            {
                                // ReSharper disable once AccessToDisposedClosure
                                outputWaitHandle.Set();
                            }
                            else
                            {
                                output.AppendLine(e.Data);
                            }
                        }

                        void ErrorHandler(object sender, DataReceivedEventArgs e)
                        {
                            if (e.Data == null)
                            {
                                // ReSharper disable once AccessToDisposedClosure
                                errorWaitHandle.Set();
                            }
                            else
                            {
                                error.AppendLine(e.Data);
                            }
                        }

                        process.OutputDataReceived += OutputHandler;
                        process.ErrorDataReceived += ErrorHandler;

                        Task<int> tskAsyncProcess = null;
                        try
                        {
                            CancellationTokenSource objCancellationTokenSource = null;
                            if (blnSync)
                                process.Start();
                            else
                            {
                                objCancellationTokenSource = new CancellationTokenSource(environment.Timeout);
#pragma warning disable AsyncFixer04 // Fire-and-forget async call inside a using block
                                tskAsyncProcess = process.StartAsync(objCancellationTokenSource.Token);
#pragma warning restore AsyncFixer04 // Fire-and-forget async call inside a using block
                            }

                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();

                            if (document.Html != null)
                            {
                                using (StreamWriter stream = process.StandardInput)
                                {
                                    byte[] buffer = Encoding.UTF8.GetBytes(document.Html);
                                    if (blnSync)
                                    {
                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                        stream.BaseStream.Write(buffer, 0, buffer.Length);
                                        // ReSharper disable once MethodHasAsyncOverload
                                        stream.WriteLine();
                                    }
                                    else
                                    {
                                        try
                                        {
                                            await stream.BaseStream.WriteAsync(
                                                buffer, 0, buffer.Length, objCancellationTokenSource.Token);
                                            await stream.WriteLineAsync();
                                        }
                                        catch (TaskCanceledException)
                                        {
                                            // Swallow this
                                        }
                                    }
                                }
                            }

                            if (blnSync)
                            {
                                if (process.WaitForExit(environment.Timeout) &&
                                    outputWaitHandle.WaitOne(environment.Timeout) &&
                                    errorWaitHandle.WaitOne(environment.Timeout))
                                {
                                    if (process.ExitCode != 0 && !File.Exists(outputPdfFilePath))
                                    {
                                        throw new PdfConvertException(
                                            $"Html to PDF conversion of '{document.Url}' failed. Wkhtmltopdf output: \r\n{error}");
                                    }
                                }
                                else
                                {
                                    if (!process.HasExited)
                                        process.Kill();

                                    throw new PdfConvertTimeoutException();
                                }
                            }
                            else
                            {
                                int intTaskResult = await tskAsyncProcess;
                                if (tskAsyncProcess.IsCompleted && objCancellationTokenSource?.IsCancellationRequested != true)
                                {
                                    if (intTaskResult != 0 && !File.Exists(outputPdfFilePath))
                                    {
                                        throw new PdfConvertException(
                                            $"Html to PDF conversion of '{document.Url}' failed. Wkhtmltopdf output: \r\n{error}");
                                    }
                                }
                                else
                                {
                                    if (!process.HasExited)
                                        process.Kill();

                                    throw new PdfConvertTimeoutException();
                                }
                            }
                        }
                        finally
                        {
                            process.OutputDataReceived -= OutputHandler;
                            process.ErrorDataReceived -= ErrorHandler;
                            tskAsyncProcess?.Dispose();
                        }
                    }
                }

                if (woutput.OutputStream != null)
                {
                    using (FileStream fs = new FileStream(outputPdfFilePath, FileMode.Open))
                    {
                        byte[] buffer = ArrayPool<byte>.Shared.Rent(32 * 1024);
                        int read;

                        if (blnSync)
                        {
                            // ReSharper disable once MethodHasAsyncOverload
                            while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                                // ReSharper disable once MethodHasAsyncOverload
                                woutput.OutputStream.Write(buffer, 0, read);
                        }
                        else
                        {
                            while ((read = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                await woutput.OutputStream.WriteAsync(buffer, 0, read);
                        }

                        ArrayPool<byte>.Shared.Return(buffer);
                    }
                }

                if (woutput.OutputCallback != null)
                {
                    byte[] pdfFileBytes = File.ReadAllBytes(outputPdfFilePath);
                    woutput.OutputCallback(document, pdfFileBytes);
                }
            }
            finally
            {
                if (delete)
                    Utils.SafeDeleteFile(outputPdfFilePath, true);
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
    //        if (8 == IntPtr.Size || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
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
