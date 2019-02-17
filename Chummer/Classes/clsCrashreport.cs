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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Chummer
{
    class CrashReport
    {
        public static void BuildFromException(object sender, UnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached) return;

            CrashReport report = new CrashReport(Guid.NewGuid())
                    .AddDefaultData()
                    .AddData("exception.txt", e.ExceptionObject.ToString());

                Log.Kill(); //Make sure log object is not used

                try
                {
                    string strFile = Path.Combine(Application.StartupPath, "chummerlog.txt");
                    report.AddData("chummerlog.txt", new StreamReader(strFile).BaseStream);
                }
                catch(Exception ex)
                {
                    report.AddData("chummerlog.txt", ex.ToString());
                }


                //Considering doing some magic with 
                //Application.OpenForms
                //And reflection to all savefiles
                //here

                //try to include default settings file
                try
                {
                    string strFilePath = Path.Combine(Application.StartupPath, "settings", "default.xml");
                    report.AddData("default.xml", new StreamReader(strFilePath).BaseStream);
                }
                catch (Exception ex)
                {
                    report.AddData("default.xml", ex.ToString());
                }


                report.Send();
                MessageBox.Show("Crash report sent.\nPlease refer to the crash id " + report.Id);
        }

        private List<KeyValuePair<String, Stream>> values; 

        /// <summary>
        /// Unique ID for the crash report, makes a user able to refer to a specific report
        /// </summary>
        public Guid Id { get; private set; }

        private String _subject;
        public String Subject
        {
            get
            {
                if (_subject == null)
                    return Id.ToString();

                return _subject;
            }
            set { _subject = value; }
        }

        public CrashReport(Guid repordGuid)
        {
            Id = repordGuid;
            values = new List<KeyValuePair<String, Stream>>();
        }

        public CrashReport AddDefaultData()
        {
            return AddData("info.txt", DefaultInfo());
        }

        private String DefaultInfo()
        {
            StringBuilder report = new StringBuilder();

            try
            {
                //Keep this multiple places for good measure
                report.AppendFormat("Crash ID = {0:B}", Id);
                report.AppendLine();
                //We want to know what crash happened on
#if LEGACY
                report.AppendFormat("Legacy Build");
#elif DEBUG
                report.AppendFormat("Debug Build");
#else
                report.AppendFormat("Release Build");
#endif
                report.AppendLine();
                //Seconadary id for linux systems?
                if (Registry.LocalMachine != null)
                {
                    RegistryKey cv = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

                    if (cv != null)
                    {
                        if (!cv.GetValueNames().Contains("ProductId"))
                        {
                            //on 32 bit builds?
                            //cv = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows NT\CurrentVersion");

                            cv = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                        }

                        String[] keys = cv.GetValueNames();
                        report.AppendFormat("Machine ID Primary= {0}", cv.GetValue("ProductId"));
                        report.AppendLine();
                    }
                }

                report.AppendFormat("CommandLine={0}", Environment.CommandLine);
                report.AppendLine();

                report.AppendFormat("Version={0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            }
            finally
            {
            }
            return report.ToString();
        }

        public CrashReport AddData(String title, String contents)
        {
            //Convert string to stream
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(contents);
            writer.Flush();
            stream.Position = 0;

            
            return AddData(title, stream);
        }

        public CrashReport AddData(String title, Stream contents)
        {
            values.Add(new KeyValuePair<string, Stream>(title, contents));
            return this;
        }

        public bool Send()
        {
            try
            {
                //Not worried about password, but don't want to place it in clear. Not that this is going to stop anybody
                //But hopefully this barrier keeps it above the lowest hanging fruits
                String password = Encoding.ASCII.GetString(Convert.FromBase64String("Y3Jhc2hkdW1wd29yZHBhc3M="));

                MailAddress address = new MailAddress("chummercrashdumps@gmail.com");
                SmtpClient client = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(address.Address, password)
                };

                MailMessage message = new MailMessage(address, address);
                
                //Forwarding rule used instead?
                message.CC.Add("chummer5isalive+chummerdump@gmail.com");

                message.Subject = Subject;
                message.Body = DefaultInfo();

                //Compression?
                foreach (KeyValuePair<string, Stream> pair in values)
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

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
