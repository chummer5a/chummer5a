using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace CrashHandler
{
	public class CrashDumper
	{
		public List<string> FilesList => _filesList;
		public Dictionary<string, string> PretendFiles => pretendFiles;
		public Dictionary<string, string> Attributes => _attributes;
		public CrashDumperProgress Progress => _progress;
		public event CrashDumperProgressChangedEvent CrashDumperProgressChanged;
		public string WorkingDirectory { get; }

		readonly List<string> _filesList;
		private readonly Dictionary<string, string> pretendFiles;
		readonly Dictionary<string, string> _attributes;
		short procId;
		private volatile CrashDumperProgress _progress;
		private Thread worker;


		public CrashDumper(string b64Json)
		{
			if (!Deserialize(b64Json, out procId, out _filesList, out pretendFiles, out _attributes))
			{
				throw new ArgumentException();
			}

			WorkingDirectory = Path.Combine(Path.GetTempPath(), GenerateFolderName());
			Directory.CreateDirectory(WorkingDirectory);
		}

		private string GenerateFolderName()
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
			SetProgress(CrashDumperProgress.Started);
			worker = new Thread(WorkerEntryPoint) {IsBackground = true};
			worker.Start();
		}

		private void WorkerEntryPoint()
		{
			try
			{
				SetProgress(CrashDumperProgress.CreateDmp);
				if (CreateDump()) return;

				SetProgress(CrashDumperProgress.CreateFiles);
				GetValue();

				SetProgress(CrashDumperProgress.CopyFiles);
				CopyFiles();

				SetProgress(CrashDumperProgress.FinishedCollecting);


			}
			catch (Exception)
			{
				SetProgress(CrashDumperProgress.Error);
			}
		}

		private void CopyFiles()
		{
			foreach (string file in _filesList)
			{
				if(!File.Exists(file)) continue;

				string name = Path.GetFileName(file);
				string destination = Path.Combine(WorkingDirectory, name);
				File.Copy(file, destination);
			}
		}

		
			
		

		private void GetValue()
		{
			StringBuilder sb = new StringBuilder();
			foreach (KeyValuePair<string, string> keyValuePair in Attributes)
			{
				sb.Append("\"");
				sb.Append(keyValuePair.Key);
				sb.Append("\"-\"");
				sb.Append(keyValuePair.Value);
				sb.AppendLine("\"");
			}
			pretendFiles.Add("attributes.txt", sb.ToString());

			foreach (KeyValuePair<string, string> pair in PretendFiles)
			{
				File.WriteAllText(Path.Combine(WorkingDirectory, pair.Key), pair.Value);
			}
		}

		private bool CreateDump()
		{

			bool ret;
			Process process = Process.GetProcessById(procId);

			using (FileStream file = File.Create(Path.Combine(WorkingDirectory, "crashdump.dmp")))
			{

				if (DbgHlp.MiniDumpWriteDump(process.Handle, procId, file.SafeFileHandle.DangerousGetHandle(),
					MINIDUMP_TYPE.MiniDumpWithPrivateReadWriteMemory |
					MINIDUMP_TYPE.MiniDumpWithDataSegs |
					MINIDUMP_TYPE.MiniDumpWithHandleData |
					MINIDUMP_TYPE.MiniDumpWithFullMemoryInfo |
					MINIDUMP_TYPE.MiniDumpWithThreadInfo |
					MINIDUMP_TYPE.MiniDumpWithUnloadedModules, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero))
				{
					ret = false;
				}
				else
				{
					int errorNo = Marshal.GetLastWin32Error();
					ret = true;
				}

				file.Flush();
			}

			return ret;
		}

		public void StartUploading()
		{
			
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
			out Dictionary<string, string> attributes)
		{
			string json = Encoding.UTF8.GetString(Convert.FromBase64String(base64json));

			object obj = new JavaScriptSerializer().DeserializeObject(json);

			Dictionary<string, object> parts = obj as Dictionary<string, object>;
			if (parts?["processid"] is int)
			{
				int pid = (int) parts["processid"];

				filesList = ((object[])parts["capturefiles"]).Select(x => x.ToString()).ToList();
				attributes = ((Dictionary<string, object>) parts["attributes"]).ToDictionary(x => x.Key, y => y.Value.ToString());
				pretendFiles = ((Dictionary<string, object>)parts["pretendfiles"]).ToDictionary(x => x.Key, y => y.Value.ToString());

				processId = (short) pid;

				return true;
			}


			processId = 0;
			filesList = null;
			pretendFiles = null;
			attributes = null;

			return false;
		}
	}

	public delegate void CrashDumperProgressChangedEvent(object sender, CrashDumperProgressChangedEventArgs args);

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

		[Description("Saving crash dump")]
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