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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Chummer;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;

namespace CrashHandler
{
    public sealed class CrashDumper : IDisposable
    {
        public Dictionary<string, string> PretendFiles => _lstPretendFilePaths;
        public Dictionary<string, string> Attributes => _attributes;
        public CrashDumperProgress Progress => _eProgress;

        public event Action<object, CrashDumperProgressChangedEventArgs> CrashDumperProgressChanged;

        public string WorkingDirectory { get; }
        public Process Process { get; private set; }

        private readonly Dictionary<string, string> _dicFilePaths;
        private readonly Dictionary<string, string> _lstPretendFilePaths;
        private readonly Dictionary<string, string> _attributes;
        private readonly int _procId;
        private readonly IntPtr _exceptionPrt;
        private readonly uint _threadId;
        private volatile CrashDumperProgress _eProgress;
        private readonly BackgroundWorker _worker = new BackgroundWorker();

        private readonly StreamWriter CrashLogWriter;
        private bool _blnDumpCreationSuccessful;

        /// <summary>
        /// Start up the crash dump collector from a Base64-encoded string containing the serialized info for the crash.
        /// </summary>
        /// <param name="strJsonPath">String path of the text file that contains our JSON package.</param>
        /// <param name="strDateString">String for the Utc Date and Time at which the crash happened.</param>
        public CrashDumper(string strJsonPath, string strDateString)
        {
            CrashDumpName = "chummer_crash_" + strDateString;
            CrashDumpLogName = Path.Combine(Utils.GetStartupPath, CrashDumpName + ".log");

            if (!Deserialize(strJsonPath, out _procId, out _dicFilePaths, out _lstPretendFilePaths, out _attributes,
                             out _threadId, out _exceptionPrt))
            {
                throw new ArgumentException("Could not deserialize");
            }

            CrashLogWriter = new StreamWriter(CrashDumpLogName, false, Encoding.UTF8);

            CrashLogWriter.WriteLine("This file contains information on a crash report for Chummer5A.");
            CrashLogWriter.WriteLine("You can safely delete this file, but a developer might want to look at it.");

            if (_lstPretendFilePaths.TryGetValue("exception.txt", out string exception))
            {
                CrashLogWriter.WriteLine(exception);
                CrashLogWriter.Flush();
            }

            CrashLogWriter.WriteLine("Crash id is " + _attributes["visible-crash-id"]);
            CrashLogWriter.Flush();

            WorkingDirectory = Path.Combine(Utils.GetTempPath(), CrashDumpName);
            Directory.CreateDirectory(WorkingDirectory);

            Attributes["visible-crashhandler-major-minor"] = "v3_0";

            CrashLogWriter.WriteLine("Crash working directory is " + WorkingDirectory);
            CrashLogWriter.Flush();

            _worker.WorkerReportsProgress = false;
            _worker.WorkerSupportsCancellation = false;
            _worker.DoWork += CollectCrashDump;
            _worker.RunWorkerCompleted += SetProgressFinishedIfAppropriate;
        }

        private void AttemptDebug(Process objProcess)
        {
            bool blnSuccess = NativeMethods.DebugActiveProcess(new IntPtr(objProcess.Id));
            int intLastError = Marshal.GetLastWin32Error();
            if (blnSuccess)
            {
                Attributes["debugger-attached-success"] = bool.TrueString;
            }
            else
            {
                Attributes["debugger-attached-error"] = new Win32Exception(intLastError).Message;
            }
        }

        private string CrashDumpName { get; }

        private string CrashDumpLogName { get; }

        private void SetProgress(CrashDumperProgress progress)
        {
            _eProgress = progress;
            CrashDumperProgressChanged?.Invoke(this, new CrashDumperProgressChangedEventArgs(progress));
        }

        public void StartCollecting()
        {
            if (!_worker.IsBusy)
                _worker.RunWorkerAsync();
        }

        private async void CollectCrashDump(object sender, DoWorkEventArgs e)
        {
            SetProgress(CrashDumperProgress.Started);
            await CrashLogWriter.WriteLineAsync("Starting dump collection").ConfigureAwait(false);
            await CrashLogWriter.FlushAsync().ConfigureAwait(false);
            try
            {
                Process = Process.GetProcessById(_procId);

                SetProgress(CrashDumperProgress.Debugger);
                await CrashLogWriter.WriteLineAsync("Attempting to attach debugger").ConfigureAwait(false);
                await CrashLogWriter.FlushAsync().ConfigureAwait(false);
                AttemptDebug(Process);
                await CrashLogWriter.WriteLineAsync("Debugger handled").ConfigureAwait(false);
                await CrashLogWriter.FlushAsync().ConfigureAwait(false);
                SetProgress(CrashDumperProgress.CreateDmp);
                await CrashLogWriter.WriteLineAsync("Creating minidump").ConfigureAwait(false);
                await CrashLogWriter.FlushAsync().ConfigureAwait(false);
                if (!await CreateDump(Process, _exceptionPrt, _threadId, Attributes.ContainsKey("debugger-attached-success")).ConfigureAwait(false))
                {
                    await CrashLogWriter.WriteLineAsync("Failed to create minidump, aborting").ConfigureAwait(false);
                    await CrashLogWriter.FlushAsync().ConfigureAwait(false);
                    SetProgress(CrashDumperProgress.Error);
                    return;
                }

                await CrashLogWriter.WriteLineAsync("Successfully created minidump").ConfigureAwait(false);
                await CrashLogWriter.FlushAsync().ConfigureAwait(false);
                SetProgress(CrashDumperProgress.CreateFiles);
                await CrashLogWriter.WriteLineAsync("Creating files containing crash information").ConfigureAwait(false);
                await CrashLogWriter.FlushAsync().ConfigureAwait(false);
                GetValue();
                await CrashLogWriter.WriteLineAsync("Successfully created files containing crash information").ConfigureAwait(false);
                await CrashLogWriter.FlushAsync().ConfigureAwait(false);
                SetProgress(CrashDumperProgress.CopyFiles);
                await CrashLogWriter.WriteLineAsync("Copying all needed files to working directory").ConfigureAwait(false);
                await CrashLogWriter.FlushAsync().ConfigureAwait(false);
                CopyFiles();
                await CrashLogWriter.WriteLineAsync("Files collected").ConfigureAwait(false);
                await CrashLogWriter.FlushAsync().ConfigureAwait(false);
                SetProgress(CrashDumperProgress.Compressing);
                await CrashLogWriter.WriteLineAsync("Creating .zip file").ConfigureAwait(false);
                await CrashLogWriter.FlushAsync().ConfigureAwait(false);
                await Task.Run(() => ZipFile.CreateFromDirectory(WorkingDirectory,
                                                                 Path.Combine(Utils.GetStartupPath, CrashDumpName)
                                                                 + ".zip",
                                                                 CompressionLevel.Optimal, false, Encoding.UTF8)).ConfigureAwait(false);
                await CrashLogWriter.WriteLineAsync("Zip file created").ConfigureAwait(false);
                await CrashLogWriter.FlushAsync().ConfigureAwait(false);
                e.Result = true;
                _blnDumpCreationSuccessful = true;
            }
            catch (Exception ex)
            {
                e.Result = false;
                await CrashLogWriter.WriteLineAsync(
                    "Encountered the following exception while collecting crash information, aborting").ConfigureAwait(false);
                await CrashLogWriter.WriteLineAsync(ex.ToString()).ConfigureAwait(false);
                await CrashLogWriter.FlushAsync().ConfigureAwait(false);
            }
            finally
            {
                SetProgress(CrashDumperProgress.Cleanup);
                try
                {
                    await CrashLogWriter.WriteLineAsync("Cleaning up working directory").ConfigureAwait(false);
                    await CrashLogWriter.FlushAsync().ConfigureAwait(false);
                    Clean();
                    await CrashLogWriter.WriteLineAsync("Cleanup done").ConfigureAwait(false);
                    await CrashLogWriter.FlushAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await CrashLogWriter.WriteLineAsync(
                        "Encountered the following exception while cleaning up working directory, skipping cleanup").ConfigureAwait(false);
                    await CrashLogWriter.WriteLineAsync(ex.ToString()).ConfigureAwait(false);
                    await CrashLogWriter.FlushAsync().ConfigureAwait(false);
                }
            }
        }

        private void SetProgressFinishedIfAppropriate(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e?.Result is bool blnResult && blnResult)
            {
                SetProgress(CrashDumperProgress.Finished);
            }
            else
            {
                SetProgress(CrashDumperProgress.Error);
            }
        }

        private async ValueTask<bool> CreateDump(Process process, IntPtr exceptionInfo, uint threadId, bool debugger, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            bool blnSuccess = false;
            using (FileStream file = File.Create(Path.Combine(WorkingDirectory, CrashDumpName + ".dmp")))
            {
                NativeMethods.MiniDumpExceptionInformation info = new NativeMethods.MiniDumpExceptionInformation
                {
                    ClientPointers = true,
                    ExceptionPointers = exceptionInfo,
                    ThreadId = threadId
                };

                const NativeMethods.MINIDUMP_TYPE dtype = NativeMethods.MINIDUMP_TYPE.MiniDumpWithPrivateReadWriteMemory |
                                                          NativeMethods.MINIDUMP_TYPE.MiniDumpWithDataSegs |
                                                          NativeMethods.MINIDUMP_TYPE.MiniDumpWithHandleData |
                                                          NativeMethods.MINIDUMP_TYPE.MiniDumpWithFullMemoryInfo |
                                                          NativeMethods.MINIDUMP_TYPE.MiniDumpWithThreadInfo |
                                                          NativeMethods.MINIDUMP_TYPE.MiniDumpWithUnloadedModules;

                bool extraInfo = !(exceptionInfo == IntPtr.Zero || threadId == 0 || !debugger);

                using (SafeFileHandle objHandle = file.SafeFileHandle)
                {
                    if (extraInfo)
                    {
                        blnSuccess = NativeMethods.MiniDumpWriteDump(process.Handle, (uint)_procId, objHandle, dtype,
                                                                     ref info, IntPtr.Zero, IntPtr.Zero);
                    }
                    else if (NativeMethods.MiniDumpWriteDump(process.Handle, (uint)_procId, objHandle, dtype, IntPtr.Zero,
                                                             IntPtr.Zero, IntPtr.Zero))
                    {
                        blnSuccess = true;

                        //Might solve the problem if crashhandler stops working on remote (hah)
                        Attributes["debug-debug-exception-info"] = exceptionInfo.ToString();
                        Attributes["debug-debug-thread-id"] = threadId.ToString();
                    }
                }

                await file.FlushAsync(token).ConfigureAwait(false);
            }

            return blnSuccess;
        }

        private void GetValue()
        {
            StringBuilder sb = new StringBuilder(byte.MaxValue);
            foreach (KeyValuePair<string, string> keyValuePair in Attributes)
            {
                sb.Append('\"').Append(keyValuePair.Key).Append("\"-\"").Append(keyValuePair.Value).Append('\"').AppendLine();
            }

            _lstPretendFilePaths.Add("attributes.txt", sb.ToString());

            foreach (KeyValuePair<string, string> pair in _lstPretendFilePaths)
            {
                File.WriteAllText(Path.Combine(WorkingDirectory, pair.Key), pair.Value);
            }
        }

        private void CopyFiles()
        {
            if (_dicFilePaths?.Count > 0)
            {
                foreach (string strFilePath in _dicFilePaths.Keys)
                {
                    if ((Path.GetExtension(strFilePath) == ".exe"
                         && !strFilePath.Contains("Chummer", StringComparison.OrdinalIgnoreCase))
                        || !File.Exists(strFilePath))
                        continue;

                    string strFileName = Path.GetFileName(strFilePath) ?? string.Empty;
                    if (string.IsNullOrEmpty(strFileName))
                        continue;
                    string strDestination = Path.Combine(WorkingDirectory, strFileName);
                    File.Copy(strFilePath, strDestination);
                }
            }
        }

        private void Clean()
        {
            if (!Directory.Exists(WorkingDirectory))
                return;
            try
            {
                Directory.Delete(WorkingDirectory, true);
            }
            catch (IOException)
            {
                // swallow this
            }
            catch (UnauthorizedAccessException)
            {
                // swallow this
            }
        }

        private static bool Deserialize(string strJsonPath,
            out int intProcessId,
            out Dictionary<string, string> dicFiles,
            out Dictionary<string, string> dicPretendFiles,
            out Dictionary<string, string> dicAttributes,
            out uint uintThreadId,
            out IntPtr ptrException)
        {
            Dictionary<string, object> parts;

            using (FileStream objFileStream = new FileStream(strJsonPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader objStreamReader = new StreamReader(objFileStream))
            using (JsonReader objJsonReader = new JsonTextReader(objStreamReader))
            {
                parts = JsonSerializer.CreateDefault().Deserialize<Dictionary<string, object>>(objJsonReader);
            }

            if (parts?.TryGetValue("_intProcessId", out object objPid) == true && objPid is int pid)
            {
                dicFiles = parts["_dicCapturedFiles"] as Dictionary<string, string>;
                dicAttributes =
                    ((Dictionary<string, object>) parts["_dicAttributes"]).ToDictionary(x => x.Key,
                        y => y.Value.ToString());
                dicPretendFiles =
                    ((Dictionary<string, object>) parts["_dicPretendFiles"]).ToDictionary(x => x.Key,
                        y => y.Value.ToString());

                intProcessId = pid;
                string s = "0";
                if (parts.TryGetValue("_ptrExceptionInfo", out object objPart))
                {
                    s = objPart?.ToString() ?? "0";
                }

                ptrException = new IntPtr(int.Parse(s));

                uintThreadId = uint.Parse(parts["_uintThreadId"]?.ToString() ?? "0");

                return true;
            }

            intProcessId = 0;
            dicFiles = null;
            dicPretendFiles = null;
            dicAttributes = null;
            ptrException = IntPtr.Zero;
            uintThreadId = 0;
            return false;
        }

        #region IDisposable Support

        private int _intIsDisposed;

        public bool IsDisposed => _intIsDisposed > 0;

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Interlocked.CompareExchange(ref _intIsDisposed, 1, 0) > 0)
                    return;
                CrashLogWriter?.Close();
                if (_blnDumpCreationSuccessful && File.Exists(CrashDumpLogName))
                {
                    try
                    {
                        File.Delete(CrashDumpLogName);
                    }
                    catch (IOException)
                    {
                        //swallow this
                    }
                    catch (UnauthorizedAccessException)
                    {
                        //swallow this
                    }
                }
            }
        }

        // Override destructor only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~CrashDumper() {
        //   Dispose(false);
        // }

        public void Dispose()
        {
            Dispose(true);
            // Uncomment the following line if the destructor is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }

    public class CrashDumperProgressChangedEventArgs : EventArgs
    {
        public CrashDumperProgress Progress { get; }

        public CrashDumperProgressChangedEventArgs(CrashDumperProgress progress)
        {
            Progress = progress;
        }
    }

    public enum CrashDumperProgress
    {
        [Description("Started collecting error information")]
        Started,

        [Description("Attaching Debugger")]
        Debugger,

        [Description("Saving crash dump (this might take a while)")]
        CreateDmp,

        [Description("Saving additional information")]
        CreateFiles,

        [Description("Copying relevant files to a temporary location")]
        CopyFiles,

        [Description("Creating .zip file of all crash information")]
        Compressing,

        [Description("Cleaning temporary files")]
        Cleanup,

        [Description("Finished creating .zip file containing all crash information")]
        Finished,

        [Description("An error happened while collecting crash information")]
        Error
    }
}
