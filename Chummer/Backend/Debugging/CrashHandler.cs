using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Chummer.Backend.Debugging
{


    internal class CrashHandler
    {
        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        private class DumpData
        {
            public DumpData()
            {
                AddDefaultInfo();
            }

            public List<string> capturefiles = new List<string>();
            public Dictionary<string, string> pretendfiles = new Dictionary<string, string>();
            public Dictionary<string, string> attributes = new Dictionary<string, string>();
            public int processid = Process.GetCurrentProcess().Id;
            public uint threadId = GetCurrentThreadId();
            public IntPtr exceptionPrt = IntPtr.Zero;

            void AddDefaultInfo()
            {
                //Crash handler will make visible-{whatever} visible in the upload while the rest will exists in a file named attributes.txt
                attributes.Add("visible-crash-id", Guid.NewGuid().ToString());

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

                    if (cv != null)
                    {
                        if (!cv.GetValueNames().Contains("ProductId"))
                        {
                            //On 32 bit builds? get 64 bit registry
                            cv = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                        }

                        attributes.Add("machine-id", cv.GetValue("ProductId").ToString());
                        attributes.Add("os-name", cv.GetValue("ProductName").ToString());
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
                exceptionPrt = Marshal.GetExceptionPointers();

                pretendfiles.Add("exception.txt", ex.ToString());

                attributes["visible-error-friendly"] = ex.Message;
            }

            public string SerializeBase64()
            {
                string altson = new JavaScriptSerializer().Serialize(this);
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(altson));
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

        internal static void WebMiniDumpHandler(Exception ex)
        {
            try
            {
                DumpData dump = new DumpData();

                dump.AddException(ex);
                dump.AddFile(Path.Combine(Application.StartupPath, "settings", "default.xml"));
                dump.AddFile(Path.Combine(Application.StartupPath, "chummerlog.txt"));

                Process crashHandler = Process.Start("crashhandler", "crash " + dump.SerializeBase64());

                crashHandler.WaitForExit();
            }
            catch(Exception nex)
            {
                MessageBox.Show("Failed to create crash report.\nHere is some information to help the developers figure out why\n" +nex + "\nCrash information:\n"+ ex);
            }
        }
    }
}