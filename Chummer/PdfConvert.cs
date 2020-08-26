using System;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;

namespace Codaxy.WkHtmlToPdf
{
    public class PdfConvertException : Exception
    {
        public PdfConvertException(string msg) : base(msg) { }
    }

    public class PdfConvertTimeoutException : PdfConvertException
    {
        public PdfConvertTimeoutException() : base("HTML to PDF conversion process has not finished in the given period.") { }
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

        public Dictionary<string, string> Cookies { get; } = new Dictionary<string, string>();
        public Dictionary<string, string> ExtraParams { get; } = new Dictionary<string, string>();
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

    public class PdfConvert
    {
        static PdfConvertEnvironment _e;

        public static PdfConvertEnvironment Environment
        {
            get
            {
                return _e = _e ?? new PdfConvertEnvironment
                {
                    TempFolderPath = Path.GetTempPath(),
                    WkHtmlToPdfPath = GetWkhtmlToPdfExeLocation(),
                    Timeout = 60000
                };
            }
        }

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

            string outputPdfFilePath;
            bool delete;
            if (woutput.OutputFilePath != null)
            {
                outputPdfFilePath = woutput.OutputFilePath;
                delete = false;
            }
            else
            {
                outputPdfFilePath = Path.Combine(environment.TempFolderPath, Guid.NewGuid().ToString("D", CultureInfo.InvariantCulture) + ".pdf");
                delete = true;
            }

            if (!File.Exists(environment.WkHtmlToPdfPath))
                throw new PdfConvertException(string.Format(CultureInfo.InvariantCulture, "File '{0}' not found. Check if wkhtmltopdf application is installed.", environment.WkHtmlToPdfPath));

            StringBuilder paramsBuilder = new StringBuilder();
            paramsBuilder.Append("--page-size A4 ");

            if (!string.IsNullOrEmpty(document.HeaderUrl))
            {
                paramsBuilder.AppendFormat("--header-html {0} ", document.HeaderUrl);
                paramsBuilder.Append("--margin-top 25 ");
                paramsBuilder.Append("--header-spacing 5 ");
            }
            if (!string.IsNullOrEmpty(document.FooterUrl))
            {
                paramsBuilder.AppendFormat("--footer-html {0} ", document.FooterUrl);
                paramsBuilder.Append("--margin-bottom 25 ");
                paramsBuilder.Append("--footer-spacing 5 ");
            }
            if (!string.IsNullOrEmpty(document.HeaderLeft))
                paramsBuilder.AppendFormat("--header-left \"{0}\" ", document.HeaderLeft);

            if (!string.IsNullOrEmpty(document.HeaderCenter))
                paramsBuilder.AppendFormat("--header-center \"{0}\" ", document.HeaderCenter);

            if (!string.IsNullOrEmpty(document.HeaderRight))
                paramsBuilder.AppendFormat("--header-right \"{0}\" ", document.HeaderRight);

            if (!string.IsNullOrEmpty(document.FooterLeft))
                paramsBuilder.AppendFormat("--footer-left \"{0}\" ", document.FooterLeft);

            if (!string.IsNullOrEmpty(document.FooterCenter))
                paramsBuilder.AppendFormat("--footer-center \"{0}\" ", document.FooterCenter);

            if (!string.IsNullOrEmpty(document.FooterRight))
                paramsBuilder.AppendFormat("--footer-right \"{0}\" ", document.FooterRight);

            if (!string.IsNullOrEmpty(document.HeaderFontSize))
                paramsBuilder.AppendFormat("--header-font-size \"{0}\" ", document.HeaderFontSize);

            if (!string.IsNullOrEmpty(document.FooterFontSize))
                paramsBuilder.AppendFormat("--footer-font-size \"{0}\" ", document.FooterFontSize);

            if (!string.IsNullOrEmpty(document.HeaderFontName))
                paramsBuilder.AppendFormat("--header-font-name \"{0}\" ", document.HeaderFontName);

            if (!string.IsNullOrEmpty(document.FooterFontName))
                paramsBuilder.AppendFormat("--footer-font-name \"{0}\" ", document.FooterFontName);


            if (document.ExtraParams != null)
                foreach (var extraParam in document.ExtraParams)
                    paramsBuilder.AppendFormat("--{0} {1} ", extraParam.Key, extraParam.Value);

            if (document.Cookies != null)
                foreach (var cookie in document.Cookies)
                    paramsBuilder.AppendFormat("--cookie {0} {1} ", cookie.Key, cookie.Value);

            paramsBuilder.AppendFormat("\"{0}\" \"{1}\"", document.Url, outputPdfFilePath);

            try
            {
                StringBuilder output = new StringBuilder();
                StringBuilder error = new StringBuilder();

                using (Process process = new Process())
                {
                    process.StartInfo.FileName = environment.WkHtmlToPdfPath;
                    process.StartInfo.Arguments = paramsBuilder.ToString();
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardInput = true;

                    using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                    using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                    {
                        void OutputHandler(object sender, DataReceivedEventArgs e)
                        {
                            if (e.Data == null)
                            {
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
                                errorWaitHandle.Set();
                            }
                            else
                            {
                                error.AppendLine(e.Data);
                            }
                        }

                        process.OutputDataReceived += OutputHandler;
                        process.ErrorDataReceived += ErrorHandler;

                        try
                        {
                            process.Start();

                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();

                            if (document.Html != null)
                            {
                                using (var stream = process.StandardInput)
                                {
                                    byte[] buffer = Encoding.UTF8.GetBytes(document.Html);
                                    stream.BaseStream.Write(buffer, 0, buffer.Length);
                                    stream.WriteLine();
                                }
                            }

                            if (process.WaitForExit(environment.Timeout) && outputWaitHandle.WaitOne(environment.Timeout) && errorWaitHandle.WaitOne(environment.Timeout))
                            {
                                if (process.ExitCode != 0 && !File.Exists(outputPdfFilePath))
                                {
                                    throw new PdfConvertException(string.Format(CultureInfo.InvariantCulture, "Html to PDF conversion of '{0}' failed. Wkhtmltopdf output: \r\n{1}", document.Url, error));
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
                            process.OutputDataReceived -= OutputHandler;
                            process.ErrorDataReceived -= ErrorHandler;
                        }
                    }
                }


                if (woutput.OutputStream != null)
                {
                    using (Stream fs = new FileStream(outputPdfFilePath, FileMode.Open))
                    {
                        byte[] buffer = new byte[32 * 1024];
                        int read;

                        while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                            woutput.OutputStream.Write(buffer, 0, read);
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
                if (delete && File.Exists(outputPdfFilePath))
                    File.Delete(outputPdfFilePath);
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
    //        response.AddHeader("Content-Disposition", "attachment; filename=\"" + fi.Name + "\"");
    //    }
    //}
}
