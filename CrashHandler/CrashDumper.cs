using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace CrashHandler
{
	public sealed class CrashDumper : IDisposable
	{
		public Dictionary<string, string> PretendFiles => _lstPretendFilePaths;
		public Dictionary<string, string> Attributes => _attributes;
		public CrashDumperProgress Progress => _progress;
	    public event Action<object, CrashDumperProgressChangedEventArgs> CrashDumperProgressChanged;
		public string WorkingDirectory { get; }
		public Process Process { get; private set; }
		public bool DoCleanUp { get; set; } = true;

		readonly List<string> _lstFilePaths;
		private readonly Dictionary<string, string> _lstPretendFilePaths;
		readonly Dictionary<string, string> _attributes;
	    readonly short _procId;
		private readonly IntPtr _exceptionPrt;
		private readonly uint _threadId;
		private volatile CrashDumperProgress _progress;
		private readonly BackgroundWorker _worker = new BackgroundWorker();
		private readonly ManualResetEvent _startSendEvent = new ManualResetEvent(false);
        private string _strLatestDumpName = string.Empty;

	    private readonly TextWriter CrashLogWriter;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b64Json">String path of the text file that contains our JSON package.</param>
		public CrashDumper(string b64Json)
		{
		    CrashLogWriter = new StreamWriter(
                Path.ChangeExtension(
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                        GenerateFolderName()
                    ),
                    "txt"), false, Encoding.UTF8);

		    CrashLogWriter.WriteLine("This file contains information on a crash report for Chummer5A.");
		    CrashLogWriter.WriteLine("You can safely delete this file, but a developer might want to look at it.");

			if (!Deserialize(b64Json, out _procId, out _lstFilePaths, out _lstPretendFilePaths, out _attributes, out _threadId, out _exceptionPrt))
			{
				throw new ArgumentException("Could not deserialize");
			}

		    if (_lstPretendFilePaths.TryGetValue("exception.txt", out string exception))
		    {
		        CrashLogWriter.WriteLine(exception);
		        CrashLogWriter.Flush();
            }

		    CrashLogWriter.WriteLine("Crash id is {0}", _attributes["visible-crash-id"]);
            CrashLogWriter.Flush();

            //		    _filesList = new List<string>();
            //		    _attributes = new Dictionary<string, string>()
            //		    {
            //		        {"visible-error-friendly", "FAKE"},
            //		        {"visible-crash-id", Guid.NewGuid().ToString()}
            //		    };
            //		    _pretendFiles = new Dictionary<string, string>();

            WorkingDirectory = Path.Combine(Path.GetTempPath(), GenerateFolderName());
			Directory.CreateDirectory(WorkingDirectory);
			Attributes["visible-crashhandler-major-minor"] = "v3_0";

            CrashLogWriter.WriteLine("Crash working directory is {0}", WorkingDirectory);
            CrashLogWriter.Flush();

            _worker.WorkerReportsProgress = false;
            _worker.WorkerSupportsCancellation = false;
            _worker.DoWork += CollectCrashDump;
        }

        private void AttemptDebug(Process process)
		{
			bool sucess = NativeMethods.DebugActiveProcess(new IntPtr(process.Id));

            int intLastError = Marshal.GetLastWin32Error();
            if (sucess)
			{
				Attributes["debugger-attached-success"] = bool.TrueString;
			}
			else
			{
                Attributes["debugger-attached-error"] = new Win32Exception(intLastError).Message;
			}
		}

		public void AllowSending()
		{
			_startSendEvent.Set();
		}

		private static string GenerateFolderName()
		{
			return $"chummer_crash_{DateTime.UtcNow.ToFileTimeUtc()}";
		}

		private void SetProgress(CrashDumperProgress progress)
		{
			_progress = progress;
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
            try
            {
                Process = Process.GetProcessById(_procId);

                SetProgress(CrashDumperProgress.Debugger);
                AttemptDebug(Process);

                SetProgress(CrashDumperProgress.CreateDmp);
                if (CreateDump(Process, _exceptionPrt, _threadId, Attributes.ContainsKey("debugger-attached-success")))
                {
                    Process.Kill();
                    SetProgress(CrashDumperProgress.Error);
                    return;
                }

                SetProgress(CrashDumperProgress.CreateFiles);
                GetValue();

                SetProgress(CrashDumperProgress.CopyFiles);
                CopyFiles();

                SetProgress(CrashDumperProgress.FinishedCollecting);

                _startSendEvent.WaitOne();

                await CrashLogWriter.WriteLineAsync("Files collected");
                await CrashLogWriter.FlushAsync();

                SetProgress(CrashDumperProgress.Compressing);
                byte[] zip = GetZip();

                SetProgress(CrashDumperProgress.Encrypting);
                byte[] encrypted = Encrypt(zip, out byte[] iv, out byte[] key);

                SetProgress(CrashDumperProgress.Uploading);
                string location = await Upload(encrypted);

                await CrashLogWriter.WriteLineAsync("Files uploaded");
                await CrashLogWriter.FlushAsync();

                SetProgress(CrashDumperProgress.Saving);
                Attributes["visible-key"] = MakeStringKey(iv, key);
                Attributes["visible-location"] = location;

                UploadToAws();

                await CrashLogWriter.WriteLineAsync("Key saved");
                await CrashLogWriter.FlushAsync();

                SetProgress(CrashDumperProgress.Cleanup);
                if (DoCleanUp)
                {
                    Clean();

                    await CrashLogWriter.WriteLineAsync("Cleanup done");
                    await CrashLogWriter.FlushAsync();
                }

                SetProgress(CrashDumperProgress.FinishedSending);
                Process.Kill();
            }
            catch (WebException rex)
            {
                SetProgress(CrashDumperProgress.Error);

                MessageBox.Show("Upload service had an error."
                                + Environment.NewLine + "Reason: " + rex.Message
                                + Environment.NewLine + Environment.NewLine + "Please manually submit an issue to https://github.com/chummer5a/chummer5a/issues and attach the file \"" + _strLatestDumpName + "\" found in \"" + WorkingDirectory + "\".");
                Process?.Kill();
            }
            catch (RemoteServiceException rex)
            {
                SetProgress(CrashDumperProgress.Error);

                MessageBox.Show("Upload service had an error."
                                + Environment.NewLine + "Reason: " + rex.Message
                                + Environment.NewLine + Environment.NewLine + "Please manually submit an issue to https://github.com/chummer5a/chummer5a/issues and attach the file \"" + _strLatestDumpName + "\" found in \"" + WorkingDirectory + "\".");
                Process?.Kill();
            }
            catch (Exception ex)
            {
                SetProgress(CrashDumperProgress.Error);
                MessageBox.Show(ex.ToString());

                Process?.Kill();
            }
        }

        private bool CreateDump(Process process, IntPtr exceptionInfo, uint threadId, bool debugger)
		{

            bool ret;
            _strLatestDumpName = "crashdump-" + DateTime.Now.ToFileTimeUtc().ToString() + ".dmp";
            using (FileStream file = File.Create(Path.Combine(WorkingDirectory, _strLatestDumpName)))
			{
                MiniDumpExceptionInformation info = new MiniDumpExceptionInformation
                {
                    ClientPointers = true,
                    ExceptionPointers = exceptionInfo,
                    ThreadId = threadId
                };

                MINIDUMP_TYPE dtype = MINIDUMP_TYPE.MiniDumpWithPrivateReadWriteMemory |
				                      MINIDUMP_TYPE.MiniDumpWithDataSegs |
				                      MINIDUMP_TYPE.MiniDumpWithHandleData |
				                      MINIDUMP_TYPE.MiniDumpWithFullMemoryInfo |
				                      MINIDUMP_TYPE.MiniDumpWithThreadInfo |
				                      MINIDUMP_TYPE.MiniDumpWithUnloadedModules;

				bool extraInfo = !(exceptionInfo == IntPtr.Zero || threadId == 0 || !debugger);

				if (extraInfo)
				{
					ret = !NativeMethods.MiniDumpWriteDump(process.Handle, _procId, file.SafeFileHandle?.DangerousGetHandle() ?? IntPtr.Zero,
                        dtype, ref info, IntPtr.Zero, IntPtr.Zero);

				}
				else if (NativeMethods.MiniDumpWriteDump(process.Handle, _procId, file.SafeFileHandle?.DangerousGetHandle() ?? IntPtr.Zero,
					dtype, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero))
				{
					ret = false;

					//Might solve the problem if crashhandler stops working on remote (hah)
					Attributes["debug-debug-exception-info"] = exceptionInfo.ToString();
					Attributes["debug-debug-thread-id"] = threadId.ToString();
				}
				else
				{
					ret = true;
				}

				file.Flush();
			}

			return ret;
		}

		private void GetValue()
		{
			StringBuilder sb = new StringBuilder();
			foreach (KeyValuePair<string, string> keyValuePair in Attributes)
			{
				sb.Append('\"');
				sb.Append(keyValuePair.Key);
				sb.Append("\"-\"");
				sb.Append(keyValuePair.Value);
				sb.AppendLine("\"");
			}
			_lstPretendFilePaths.Add("attributes.txt", sb.ToString());

			foreach (KeyValuePair<string, string> pair in _lstPretendFilePaths)
			{
				File.WriteAllText(Path.Combine(WorkingDirectory, pair.Key), pair.Value);
			}
		}

		private void CopyFiles()
		{
		    if (_lstFilePaths?.Count > 0)
		    {
		        foreach (string strFilePath in _lstFilePaths)
		        {
		            if (!File.Exists(strFilePath))
                        continue;

		            string name = Path.GetFileName(strFilePath) ?? string.Empty;
		            string destination = Path.Combine(WorkingDirectory, name);
		            File.Copy(strFilePath, destination);
		        }
		    }
		}

		private byte[] GetZip()
		{
            byte[] objReturn;
            using (MemoryStream mem = new MemoryStream())
            {
                // archive.Dispose() should call mem.Dispose()
                using (ZipArchive archive = new ZipArchive(mem, ZipArchiveMode.Create, false))
                {
                    foreach (string file in Directory.EnumerateFiles(WorkingDirectory))
                    {
                        archive.CreateEntryFromFile(file, Path.GetFileName(file));
                    }

                    objReturn = mem.ToArray();
                }
            }
            return objReturn;
        }

		private static byte[] Encrypt(byte[] unencrypted, out byte[] Iv, out byte[] Key)
		{
			byte[] encrypted;
            // Create the streams used for encryption.
            using (AesManaged managed = new AesManaged())
            {
                Iv = managed.IV;
				Key = managed.Key;

				// Create a decryptor to perform the stream transform.
				ICryptoTransform encryptor = managed.CreateEncryptor(managed.Key, managed.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    // csEncrypt.Dispose() should call msEncrypt.Dispose()
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(unencrypted, 0, unencrypted.Length);
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

			return encrypted;
		}

	    private async Task<string> Upload(byte[] payload)
	    {
            using (HttpClient client = new HttpClient())
            {
                using (MemoryStream stream = new MemoryStream(payload))
                {
                    using (StreamContent content = new StreamContent(stream))
                    {
                        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                        using (HttpResponseMessage response = await client.PostAsync(@"http://crashdump.chummer.net/api/crashdumper/upload", content))
                        {
                            string resp = await response.Content.ReadAsStringAsync();

                            return ExtractUrl(resp);
                        }
                    }
                }
            }
        }

	    private string ExtractUrl(string input)
		{
            try
            {
                Dictionary<string, object> top = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(input);
                if ("success".Equals(top["status"]))
                {
                    Dictionary<string, object> files = (Dictionary<string, object>)((ArrayList)top["files"])[0];
                    string ret = (string)files["url"];
                    return ret;
                }
                throw new RemoteServiceException(top["reason"].ToString());
            }
            catch (ArgumentException)
            {
                throw new RemoteServiceException("Unable to connect to Crash Dump upload server.");
            }
		}

		private void UploadToAws()
		{
			Dictionary<string, string> upload = Attributes.Where(x => x.Key.StartsWith("visible-", StringComparison.Ordinal)).ToDictionary(x => x.Key.Replace("visible-","").Replace('-', '_'), x => x.Value);
			string payload = new JavaScriptSerializer().Serialize(upload);

            using (HttpClient client = new HttpClient())
            {
                using (StringContent payloadString = new StringContent(payload))
                {
                    client.PostAsync("https://ccbysveroa.execute-api.eu-central-1.amazonaws.com/prod/ChummerCrashService", payloadString);
                    //HttpResponseMessage msg = client.PostAsync("https://ccbysveroa.execute-api.eu-central-1.amazonaws.com/prod/ChummerCrashService", new StringContent(payload)).Result;

                    //string result = msg.Content.ReadAsStringAsync().Result;
                }
            }
        }

		private void Clean()
		{
			Directory.Delete(WorkingDirectory, true);
		}

		//public void StartPoint(string[] args)
		//{
		//	if (args.Length == 0)
		//	{
		//		{
		//			return ;
		//		}
		//	}

		//	 return;


		//	Process process = Process.GetProcessById(procId);

		//	FileStream file = File.Create("dump.dmp");

		//	if (DbgHlp.MiniDumpWriteDump(process.Handle, procId, file.SafeFileHandle.DangerousGetHandle(),
		//		MINIDUMP_TYPE.MiniDumpWithPrivateReadWriteMemory |
		//		MINIDUMP_TYPE.MiniDumpWithDataSegs |
		//		MINIDUMP_TYPE.MiniDumpWithHandleData |
		//		MINIDUMP_TYPE.MiniDumpWithFullMemoryInfo |
		//		MINIDUMP_TYPE.MiniDumpWithThreadInfo |
		//		MINIDUMP_TYPE.MiniDumpWithUnloadedModules, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero))
		//	{

		//	}
		//	else
		//	{
		//		int errorNo = Marshal.GetLastWin32Error();
		//		return;
		//	}
		//}

		static bool Deserialize(string base64json,
			out short processId,
			out List<string> filesList,
			out Dictionary<string, string> pretendFiles,
			out Dictionary<string, string> attributes,
			out uint threadId,
			out IntPtr exceptionPrt)
		{

            string json = Encoding.UTF8.GetString(File.ReadAllBytes(base64json));
		    byte[] tempBytes = Convert.FromBase64String(json);
            object obj = new JavaScriptSerializer().DeserializeObject(Encoding.UTF8.GetString(tempBytes));

			Dictionary<string, object> parts = obj as Dictionary<string, object>;
			if (parts?["_intProcessId"] is int pid)
			{
			    filesList = parts["_dicCapturedFiles"] as List<string>;
				attributes = ((Dictionary<string, object>) parts["_dicAttributes"]).ToDictionary(x => x.Key, y => y.Value.ToString());
				pretendFiles = ((Dictionary<string, object>)parts["_dicPretendFiles"]).ToDictionary(x => x.Key, y => y.Value.ToString());

				processId = (short) pid;
			    string s = "0";
                if (parts.ContainsKey("exceptionPrt"))
			    {
			        s = parts["exceptionPrt"]?.ToString() ?? "0";
                }

				exceptionPrt = new IntPtr(int.Parse(s));

				threadId = uint.Parse(parts["_uintThreadId"]?.ToString() ?? "0");

				return true;
			}


			processId = 0;
			filesList = null;
			pretendFiles = null;
			attributes = null;
			exceptionPrt = IntPtr.Zero;
			threadId = 0;
			return false;
		}

		private static string MakeStringKey(byte[] iv, byte[] key)
		{
			return string.Join("", iv.Select(x => x.ToString("X2"))) + ":" + string.Join("", key.Select(x => x.ToString("X2")));
		}

        #region IDisposable Support
        private bool disposedValue; // To detect redundant calls

	    private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _startSendEvent.Dispose();
                    CrashLogWriter?.Close();
                }

                disposedValue = true;
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
        #endregion
    }

    [Serializable]
    public sealed class RemoteServiceException : Exception
    {
        public RemoteServiceException(string toString) : base(toString)
        {

        }
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

		[Description("Copying relevant files")]
		CopyFiles,

		[Description("Ready to send crash information")]
		FinishedCollecting,

		[Description("Compressing crash information")]
		Compressing,

		[Description("Encrypting crash information")]
		Encrypting,

		[Description("Uploading (this might take a while)")]
		Uploading,

		[Description("Saving and verifying")]
		Saving,

		[Description("Cleaning temporary files")]
		Cleanup,

		[Description("Finished")]
		FinishedSending,

		[Description("An error happened while collecting crash information")]
		Error
	}
}
