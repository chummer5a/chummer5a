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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Win32;

namespace Chummer
{
    public sealed class CrashReportData
    {
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        private static void BuildFromException(object sender, UnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
                return;

            CrashReportData report = new CrashReportData(Guid.NewGuid()).AddDefaultData().AddData("exception.txt", e.ExceptionObject.ToString());

            //Log.IsLoggerEnabled = false; //Make sure log object is not used

            try
            {
                string strFile = Path.Combine(Utils.GetStartupPath, "chummerlog.txt");
                using (StreamReader objStream = new StreamReader(strFile, Encoding.UTF8, true))
                    report.AddData("chummerlog.txt", objStream.BaseStream);
            }
            catch (Exception ex)
            {
                report.AddData("chummerlog.txt", ex.ToString());
            }

            //Considering doing some magic with
            //Application.OpenForms
            //And reflection to all savefiles
            //here

            //try to include all settings files
            foreach (string strSettingFile in Directory.EnumerateFiles(Path.Combine(Utils.GetStartupPath, "settings"), "*.xml"))
            {
                string strName = Path.GetFileName(strSettingFile);
                try
                {
                    using (StreamReader objStream = new StreamReader(strSettingFile, Encoding.UTF8, true))
                        report.AddData(strName, objStream.BaseStream);
                }
                catch (Exception ex)
                {
                    report.AddData(strName, ex.ToString());
                }
            }

            report.Send();
            Program.ShowMessageBox("Crash report sent." + Environment.NewLine + "Please refer to the crash id " + report.Id);
        }

        private readonly List<KeyValuePair<string, Stream>> _lstValues;

        /// <summary>
        /// Unique ID for the crash report, makes a user able to refer to a specific report
        /// </summary>
        public Guid Id { get; }

        private string _subject;

        public string Subject
        {
            get => _subject ?? Id.ToString("D", GlobalSettings.InvariantCultureInfo);
            set => _subject = value;
        }

        public CrashReportData(Guid repordGuid)
        {
            Id = repordGuid;
            _lstValues = new List<KeyValuePair<string, Stream>>(1);
        }

        public CrashReportData AddDefaultData()
        {
            return AddData("info.txt", DefaultInfo());
        }

        private string DefaultInfo()
        {
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder report))
            {
                try
                {
                    //Keep this multiple places for good measure
                    report.AppendFormat("Crash ID = {0:B}", Id);
                    report.AppendLine();
                    //We want to know what crash happened on
#if DEBUG
                    report.AppendLine("Debug Build");
#else
                    report.AppendLine("Release Build");
#endif
                    //Secondary id for linux systems?
                    if (Registry.LocalMachine != null)
                    {
                        RegistryKey cv
                            = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                        RegistryKey cv2 = null;

                        if (cv != null)
                        {
                            if (!cv.GetValueNames().Contains("ProductId"))
                            {
                                //on 32 bit builds?
                                //cv = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows NT\CurrentVersion");
                                cv.Close();
                                cv2 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                                cv = cv2.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                            }

                            if (cv != null)
                            {
                                report.Append("Machine ID Primary=").AppendLine(cv.GetValue("ProductId").ToString());
                            }
                        }

                        cv?.Close();
                        cv2?.Close();
                    }

                    report.Append("CommandLine=").AppendLine(Environment.CommandLine);

                    report.Append("Version=").Append(Utils.CurrentChummerVersion);
                }
                catch (Exception ex)
                {
                    report.AppendLine();
                    report.Append("CrashHandlerException=").Append(ex);
                }

                return report.ToString();
            }
        }

        public CrashReportData AddData(string title, string contents)
        {
            //Convert string to stream
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(contents);
                writer.Flush();
                stream.Position = 0;

                return AddData(title, stream);
            }
        }

        public CrashReportData AddData(string title, Stream contents)
        {
            _lstValues.Add(new KeyValuePair<string, Stream>(title, contents));
            return this;
        }

        public bool Send()
        {
            try
            {
                //Not worried about password, but don't want to place it in clear. Not that this is going to stop anybody
                //But hopefully this barrier keeps it above the lowest hanging fruits
                string password = Encoding.ASCII.GetString(Convert.FromBase64String("Y3Jhc2hkdW1wd29yZHBhc3M="));

                MailAddress address = new MailAddress("chummercrashdumps@gmail.com");
                using (SmtpClient client = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(address.Address, password)
                })
                {
                    using (MailMessage message = new MailMessage(address, address))
                    {
                        //Forwarding rule used instead?
                        message.CC.Add("chummer5isalive+chummerdump@gmail.com");

                        message.Subject = Subject;
                        message.Body = DefaultInfo();

                        //Compression?
                        foreach (KeyValuePair<string, Stream> pair in _lstValues)
                        {
                            message.Attachments.Add(new Attachment(pair.Value, pair.Key));
                        }

                        if (Debugger.IsAttached)
                        {
                            Debugger.Break();
                        }
                        else
                        {
                            client.Send(message);
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
