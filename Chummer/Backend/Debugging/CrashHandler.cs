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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Chummer.Backend
{
    public static class CrashHandler
    {
        private static class NativeMethods
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
            internal static extern uint GetCurrentThreadId();
        }

        private sealed class DumpData
        {
            public DumpData()
            {
                AddDefaultInfo();
            }

            // JavaScriptSerializer requires that all properties it accesses be public.
            // ReSharper disable once MemberCanBePrivate.Local 
            public readonly List<string> capturefiles = new List<string>();
            // ReSharper disable once MemberCanBePrivate.Local 
            public readonly Dictionary<string, string> pretendfiles = new Dictionary<string, string>();
            // ReSharper disable once MemberCanBePrivate.Local 
            public readonly Dictionary<string, string> attributes = new Dictionary<string, string>();
            public int processid = Process.GetCurrentProcess().Id;
            public uint threadId = NativeMethods.GetCurrentThreadId();

            void AddDefaultInfo()
            {
                //Crash handler will make visible-{whatever} visible in the upload while the rest will exists in a file named attributes.txt
                attributes.Add("visible-crash-id", Guid.NewGuid().ToString("D"));

                attributes.Add("visible-build-type",
                    #if DEBUG
                    "DEBUG"
                    #else
                    "RELEASE"
                    #endif
                    );
                attributes.Add("commandline", Environment.CommandLine);
                attributes.Add("visible-version", Application.ProductVersion);

                if (Registry.LocalMachine != null)
                {
                    RegistryKey cv = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

                    if (cv?.GetValueNames().Contains("ProductId") == false)
                    {
                        //On 32 bit builds? get 64 bit registry
                        cv.Close();
                        cv = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                    }

                    if (cv != null)
                    {
                        attributes.Add("machine-id", cv.GetValue("ProductId").ToString());
                        attributes.Add("os-name", cv.GetValue("ProductName").ToString());

                        cv.Close();
                    }
                }

                attributes.Add("machine-name", Environment.MachineName);
                attributes.Add("current-dir", Application.StartupPath);
                attributes.Add("application-dir", Application.ExecutablePath);
                attributes.Add("os-type", Environment.OSVersion.VersionString);


                attributes.Add("visible-error-friendly", "No description available");

                PropertyInfo[] systemInformation = typeof(SystemInformation).GetProperties();
                foreach (PropertyInfo propertyInfo in systemInformation)
                {
                    attributes.Add("system-info-"+ propertyInfo.Name, propertyInfo.GetValue(null).ToString());
                }
            }

            public void AddException(Exception ex)
            {
                pretendfiles.Add("exception.txt", ex.ToString());

                attributes["visible-error-friendly"] = ex.Message;
            }

            public string SerializeBase64()
            {
                string altson = new JavaScriptSerializer().Serialize(this);
                string sReturn = Convert.ToBase64String(Encoding.UTF8.GetBytes(altson));
                return sReturn;
            }

            public void AddFile(string file)
            {
                capturefiles.Add(file);
            }

            public void AddPrentendFile(string filename, string contents)
            {
                pretendfiles.Add(filename, contents);
            }
        }

        public static void WebMiniDumpHandler(Exception ex)
        {
            try
            {
                DumpData dump = new DumpData();

                dump.AddException(ex);
                dump.AddFile(Path.Combine(Application.StartupPath, "settings", "default.xml"));
                dump.AddFile(Path.Combine(Application.StartupPath, "chummerlog.txt"));

                byte[] info = new UTF8Encoding(true).GetBytes(dump.SerializeBase64());
                File.WriteAllBytes(Path.Combine(Application.StartupPath, "json.txt"), info);

                //Process crashHandler = Process.Start("crashhandler", "crash " + Path.Combine(Application.StartupPath, "json.txt") + " --debug");
                Process crashHandler = Process.Start("crashhandler", "crash " + Path.Combine(Application.StartupPath, "json.txt"));

                crashHandler?.WaitForExit();
            }
            catch(Exception nex)
            {
                MessageBox.Show("Failed to create crash report." + Environment.NewLine +
                                "Here is some information to help the developers figure out why:" + Environment.NewLine + nex + Environment.NewLine + "Crash information:" + Environment.NewLine + ex);
            }
        }
    }
}
