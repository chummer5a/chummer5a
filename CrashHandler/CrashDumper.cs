using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Script.Serialization;
using Chummer;
using Microsoft.Win32.SafeHandles;

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

        private readonly ConcurrentDictionary<string, string> _dicFilePaths;
        private readonly Dictionary<string, string> _lstPretendFilePaths;
        private readonly Dictionary<string, string> _attributes;
        private readonly short _procId;
        private readonly IntPtr _exceptionPrt;
        private readonly uint _threadId;
        private volatile CrashDumperProgress _eProgress;
        private readonly BackgroundWorker _worker = new BackgroundWorker();

        private readonly StreamWriter CrashLogWriter;
        private bool _blnDumpCreationSuccessful;

        /// <summary>
        /// Start up the crash dump collector from a Base64-encoded string containing the serialized info for the crash.
        /// </summary>
        /// <param name="strBase64Json">String path of the text file that contains our JSON package.</param>
        public CrashDumper(string strBase64Json)
        {
            CrashLogWriter = new StreamWriter(CrashDumpLogName, false, Encoding.UTF8);

            CrashLogWriter.WriteLine("This file contains information on a crash report for Chummer5A.");
            CrashLogWriter.WriteLine("You can safely delete this file, but a developer might want to look at it.");

            if (!Deserialize(strBase64Json, out _procId, out _dicFilePaths, out _lstPretendFilePaths, out _attributes,
                out _threadId, out _exceptionPrt))
            {
                throw new ArgumentException("Could not deserialize");
            }

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

        private static string CrashDumpName { get; } = "chummer_crash_" + DateTime.UtcNow.ToFileTimeUtc();

        private static string CrashDumpLogName { get; } = Path.Combine(Utils.GetStartupPath, CrashDumpName + ".log");

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
            await CrashLogWriter.WriteLineAsync("Starting dump collection");
            await CrashLogWriter.FlushAsync();
            try
            {
                Process = Process.GetProcessById(_procId);

                SetProgress(CrashDumperProgress.Debugger);
                await CrashLogWriter.WriteLineAsync("Attempting to attach debugger");
                await CrashLogWriter.FlushAsync();
                AttemptDebug(Process);
                await CrashLogWriter.WriteLineAsync("Debugger handled");
                await CrashLogWriter.FlushAsync();
                SetProgress(CrashDumperProgress.CreateDmp);
                await CrashLogWriter.WriteLineAsync("Creating minidump");
                await CrashLogWriter.FlushAsync();
                if (!CreateDump(Process, _exceptionPrt, _threadId, Attributes.ContainsKey("debugger-attached-success")))
                {
                    await CrashLogWriter.WriteLineAsync("Failed to create minidump, aborting");
                    await CrashLogWriter.FlushAsync();
                    SetProgress(CrashDumperProgress.Error);
                    return;
                }

                await CrashLogWriter.WriteLineAsync("Successfully created minidump");
                await CrashLogWriter.FlushAsync();
                SetProgress(CrashDumperProgress.CreateFiles);
                await CrashLogWriter.WriteLineAsync("Creating files containing crash information");
                await CrashLogWriter.FlushAsync();
                GetValue();
                await CrashLogWriter.WriteLineAsync("Successfully created files containing crash information");
                await CrashLogWriter.FlushAsync();
                SetProgress(CrashDumperProgress.CopyFiles);
                await CrashLogWriter.WriteLineAsync("Copying all needed files to working directory");
                await CrashLogWriter.FlushAsync();
                CopyFiles();
                await CrashLogWriter.WriteLineAsync("Files collected");
                await CrashLogWriter.FlushAsync();
                SetProgress(CrashDumperProgress.Compressing);
                await CrashLogWriter.WriteLineAsync("Creating .zip file");
                await CrashLogWriter.FlushAsync();
                ZipFile.CreateFromDirectory(WorkingDirectory,
                                            Path.Combine(Utils.GetStartupPath, CrashDumpName) + ".zip",
                                            CompressionLevel.Optimal, false, Encoding.UTF8);
                await CrashLogWriter.WriteLineAsync("Zip file created");
                await CrashLogWriter.FlushAsync();
                e.Result = true;
                _blnDumpCreationSuccessful = true;
            }
            catch (Exception ex)
            {
                e.Result = false;
                await CrashLogWriter.WriteLineAsync(
                    "Encountered the following exception while collecting crash information, aborting");
                await CrashLogWriter.WriteLineAsync(ex.ToString());
                await CrashLogWriter.FlushAsync();
            }
            finally
            {
                SetProgress(CrashDumperProgress.Cleanup);
                try
                {
                    await CrashLogWriter.WriteLineAsync("Cleaning up working directory");
                    await CrashLogWriter.FlushAsync();
                    Clean();
                    await CrashLogWriter.WriteLineAsync("Cleanup done");
                    await CrashLogWriter.FlushAsync();
                }
                catch (Exception ex)
                {
                    await CrashLogWriter.WriteLineAsync(
                        "Encountered the following exception while cleaning up working directory, skipping cleanup");
                    await CrashLogWriter.WriteLineAsync(ex.ToString());
                    await CrashLogWriter.FlushAsync();
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

        private bool CreateDump(Process process, IntPtr exceptionInfo, uint threadId, bool debugger)
        {
            bool blnSuccess = false;
            using (FileStream file = File.Create(Path.Combine(WorkingDirectory, CrashDumpName + ".dmp")))
            {
                MiniDumpExceptionInformation info = new MiniDumpExceptionInformation
                {
                    ClientPointers = true,
                    ExceptionPointers = exceptionInfo,
                    ThreadId = threadId
                };

                const MINIDUMP_TYPE dtype = MINIDUMP_TYPE.MiniDumpWithPrivateReadWriteMemory |
                                            MINIDUMP_TYPE.MiniDumpWithDataSegs |
                                            MINIDUMP_TYPE.MiniDumpWithHandleData |
                                            MINIDUMP_TYPE.MiniDumpWithFullMemoryInfo |
                                            MINIDUMP_TYPE.MiniDumpWithThreadInfo |
                                            MINIDUMP_TYPE.MiniDumpWithUnloadedModules;

                bool extraInfo = !(exceptionInfo == IntPtr.Zero || threadId == 0 || !debugger);

                using (SafeFileHandle objHandle = file.SafeFileHandle)
                {
                    if (extraInfo)
                    {
                        blnSuccess = NativeMethods.MiniDumpWriteDump(process.Handle, _procId,
                                                                     objHandle?.DangerousGetHandle()
                                                                     ?? IntPtr.Zero,
                                                                     dtype, ref info, IntPtr.Zero, IntPtr.Zero);
                    }
                    else if (NativeMethods.MiniDumpWriteDump(process.Handle, _procId,
                                                             objHandle?.DangerousGetHandle() ?? IntPtr.Zero,
                                                             dtype, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero))
                    {
                        blnSuccess = true;

                        //Might solve the problem if crashhandler stops working on remote (hah)
                        Attributes["debug-debug-exception-info"] = exceptionInfo.ToString();
                        Attributes["debug-debug-thread-id"] = threadId.ToString();
                    }
                }

                file.Flush();
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

        private static bool Deserialize(string strBase64json,
            out short shrProcessId,
            out ConcurrentDictionary<string, string> dicFiles,
            out Dictionary<string, string> dicPretendFiles,
            out Dictionary<string, string> dicAttributes,
            out uint uintThreadId,
            out IntPtr ptrException)
        {
            string json = Encoding.UTF8.GetString(File.ReadAllBytes(strBase64json));
            byte[] tempBytes = Convert.FromBase64String(json);
            object obj = new JavaScriptSerializer().DeserializeObject(Encoding.UTF8.GetString(tempBytes));

            Dictionary<string, object> parts = obj as Dictionary<string, object>;
            if (parts?["_intProcessId"] is int pid)
            {
                dicFiles = parts["_dicCapturedFiles"] as ConcurrentDictionary<string, string>;
                dicAttributes =
                    ((Dictionary<string, object>) parts["_dicAttributes"]).ToDictionary(x => x.Key,
                        y => y.Value.ToString());
                dicPretendFiles =
                    ((Dictionary<string, object>) parts["_dicPretendFiles"]).ToDictionary(x => x.Key,
                        y => y.Value.ToString());

                shrProcessId = (short) pid;
                string s = "0";
                if (parts.TryGetValue("_ptrExceptionInfo", out object objPart))
                {
                    s = objPart?.ToString() ?? "0";
                }

                ptrException = new IntPtr(int.Parse(s));

                uintThreadId = uint.Parse(parts["_uintThreadId"]?.ToString() ?? "0");

                return true;
            }

            shrProcessId = 0;
            dicFiles = null;
            dicPretendFiles = null;
            dicAttributes = null;
            ptrException = IntPtr.Zero;
            uintThreadId = 0;
            return false;
        }

        #region IDisposable Support

        private bool _blnIsDisposed; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (_blnIsDisposed)
                return;
            if (disposing)
            {
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

            _blnIsDisposed = true;
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
