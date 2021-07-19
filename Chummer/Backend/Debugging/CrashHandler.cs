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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Win32;
using NLog;

namespace Chummer.Backend
{
    public static class CrashHandler
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private sealed class DumpData : ISerializable
        {
            public DumpData(Exception ex)
            {
                _dicPretendFiles = new Dictionary<string, string> { { "exception.txt", ex?.ToString() ?? "No Exception Specified" } };

                _dicAttributes = new Dictionary<string, string>
                {
                    {"visible-crash-id", Guid.NewGuid().ToString("D", GlobalOptions.InvariantCultureInfo)},
#if DEBUG
                    {"visible-build-type", "DEBUG"},
#else
                    {"visible-build-type", "RELEASE"},
#endif
                    {"commandline", Environment.CommandLine},
                    {"visible-version", Application.ProductVersion},
                    {"machine-name", Environment.MachineName},
                    {"current-dir", Utils.GetStartupPath},
                    {"application-dir", Application.ExecutablePath},
                    {"os-type", Environment.OSVersion.VersionString},
                    {"visible-error-friendly", ex?.Message ?? "No description available"},
                    { "installation-id", Properties.Settings.Default.UploadClientId.ToString() },
                    { "option-upload-logs-set", GlobalOptions.UseLoggingApplicationInsights.ToString() }
                };

                try
                {
                    _dicAttributes.Add("chummer-ui-language", GlobalOptions.Language);
                }
                catch (Exception e)
                {
                    _dicAttributes.Add("chummer-ui-language", e.ToString());
                }
                try
                {
                    _dicAttributes.Add("chummer-cultureinfo", GlobalOptions.CultureInfo.ToString());
                }
                catch (Exception e)
                {
                    _dicAttributes.Add("chummer-cultureinfo", e.ToString());
                }
                try
                {
                    _dicAttributes.Add("system-cultureinfo", GlobalOptions.SystemCultureInfo.ToString());
                }
                catch (Exception e)
                {
                    _dicAttributes.Add("system-cultureinfo", e.ToString());
                }

                //Crash handler will make visible-{whatever} visible in the upload while the rest will exists in a file named attributes.txt
                if (Registry.LocalMachine != null)
                {
                    RegistryKey obj64BitRegistryKey = null;
                    RegistryKey objCurrentVersionKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

                    if (objCurrentVersionKey?.GetValueNames().Contains("ProductId") == false)
                    {
                        //On 32 bit builds? get 64 bit registry
                        objCurrentVersionKey.Close();
                        obj64BitRegistryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                        objCurrentVersionKey = obj64BitRegistryKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                    }

                    if (objCurrentVersionKey != null)
                    {
                        try
                        {
                            _dicAttributes.Add("machine-id", objCurrentVersionKey.GetValue("ProductId").ToString());
                        }
                        catch (Exception e)
                        {
                            _dicAttributes.Add("machine-id", e.ToString());
                        }

                        try
                        {
                            _dicAttributes.Add("os-name", objCurrentVersionKey.GetValue("ProductName").ToString());
                        }
                        catch (Exception e)
                        {
                            _dicAttributes.Add("os-name", e.ToString());
                        }

                        objCurrentVersionKey.Close();
                    }
                    obj64BitRegistryKey?.Close();
                }

                PropertyInfo[] systemInformation = typeof(SystemInformation).GetProperties();
                foreach (PropertyInfo propertyInfo in systemInformation)
                {
                    _dicAttributes.Add("system-info-" + propertyInfo.Name, propertyInfo.GetValue(null).ToString());
                }
            }

            // JavaScriptSerializer requires that all properties it accesses be public.
            // ReSharper disable once MemberCanBePrivate.Local
            public readonly ConcurrentDictionary<string, string> _dicCapturedFiles = new ConcurrentDictionary<string, string>();

            // ReSharper disable once MemberCanBePrivate.Local
            public readonly Dictionary<string, string> _dicPretendFiles;

            // ReSharper disable once MemberCanBePrivate.Local
            public readonly Dictionary<string, string> _dicAttributes;

            // ReSharper disable once MemberCanBePrivate.Local
            public readonly int _intProcessId = Process.GetCurrentProcess().Id;

            // ReSharper disable once MemberCanBePrivate.Local
            public readonly uint _uintThreadId = NativeMethods.GetCurrentThreadId();

            public string SerializeBase64()
            {
                string altson = new JavaScriptSerializer().Serialize(this);
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(altson));
            }

            public void AddFile(string strFileName)
            {
                string strContents;
                try
                {
                    strContents = File.ReadAllText(strFileName);
                }
                catch (Exception e)
                {
                    strContents = e.ToString();
                }

                if (!_dicCapturedFiles.TryAdd(strFileName, strContents))
                    _dicCapturedFiles[strFileName] = strContents;
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("procesid", _intProcessId);
                info.AddValue("threadid", _uintThreadId);
                foreach (KeyValuePair<string, string> objLoopKeyValuePair in _dicAttributes)
                    info.AddValue(objLoopKeyValuePair.Key, objLoopKeyValuePair.Value);
                foreach (KeyValuePair<string, string> objLoopKeyValuePair in _dicPretendFiles)
                    info.AddValue(objLoopKeyValuePair.Key, objLoopKeyValuePair.Value);
                foreach (KeyValuePair<string, string> objLoopKeyValuePair in _dicCapturedFiles)
                    info.AddValue(objLoopKeyValuePair.Key, objLoopKeyValuePair.Value);
            }
        }

        public static void WebMiniDumpHandler(Exception ex)
        {
            try
            {
                if (GlobalOptions.UseLoggingApplicationInsights >= UseAILogging.Crashes && Program.ChummerTelemetryClient != null)
                {
                    ex.Data.Add("IsCrash", bool.TrueString);
                    ExceptionTelemetry et = new ExceptionTelemetry(ex)
                    {
                        SeverityLevel = SeverityLevel.Critical
                    };
                    //we have to enable the uploading of THIS message, so it isn't filtered out in the DropUserdataTelemetryProcessos
                    foreach (DictionaryEntry d in ex.Data)
                    {
                        if ((d.Key != null) && (d.Value != null))
                            et.Properties.Add(d.Key.ToString(), d.Value.ToString());
                    }
                    Program.ChummerTelemetryClient.TrackException(et);
                    Program.ChummerTelemetryClient.Flush();
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            try
            {
                DumpData dump = new DumpData(ex);
                foreach (string strSettingFile in Directory.EnumerateFiles(Path.Combine(Utils.GetStartupPath, "settings"), "*.xml"))
                {
                    dump.AddFile(strSettingFile);
                }
                dump.AddFile(Path.Combine(Utils.GetStartupPath, "chummerlog.txt"));

                byte[] info = new UTF8Encoding(true).GetBytes(dump.SerializeBase64());
                File.WriteAllBytes(Path.Combine(Utils.GetStartupPath, "json.txt"), info);

                //Process crashHandler = Process.Start("crashhandler", "crash " + Path.Combine(Utils.GetStartupPath, "json.txt") + " --debug");
                Process crashHandler = Process.Start("crashhandler", "crash " + Path.Combine(Utils.GetStartupPath, "json.txt"));

                crashHandler?.WaitForExit();
            }
            catch (Exception nex)
            {
                Program.MainForm.ShowMessageBox(
                    "Failed to create crash report." + Environment.NewLine +
                    "Chummer crashed with version: " + Assembly.GetAssembly(typeof(Program))?.GetName().Version + Environment.NewLine +
                    "Here is some information to help the developers figure out why:" + Environment.NewLine + nex +
                    Environment.NewLine + "Crash information:" + Environment.NewLine + ex, "Failed to Create Crash Report", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}