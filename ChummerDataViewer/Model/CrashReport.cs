using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace ChummerDataViewer.Model
{
	public class CrashReport
	{
		public EventHandler ProgressChanged;

		private readonly Database.DatabasePrivateApi _database;
		private readonly string _key;
		private string _downloadedZip;
		private string _folderlocation;

		public bool IsDownloadStarted => _downloadedZip != null;
		public bool IsUnpackStarted => _folderlocation != null;


		public Guid Guid { get; }
		
		public DateTime Timestamp { get; }
		public Version Version { get; }
		public string BuildType { get;  }
		public string ErrorFrindly { get;  }
		public string WebFileLocation { get; set; }
		public string StackTrace { get; set; }
		public string Userstory { get; set; }
		

		public CrashReportProcessingProgress Progress
		{
			get { return _progress; }
			private set
			{
				_progress = value; 
				ProgressChanged?.Invoke(this, EventArgs.Empty);
			}
		}


		internal CrashReport(Database.DatabasePrivateApi database, Guid guid, long unixTimeStamp, string buildType, string errorFrindly, string key,
			string webFileLocation, Version version, string downloadedZip = null, string folderlocation = null, string stackTrace = null, string userstory = null)
		{
			_database = database;
			_key = key;
			_downloadedZip = downloadedZip;
			_folderlocation = folderlocation;
			Guid = guid;
			BuildType = buildType;
			ErrorFrindly = errorFrindly;
			WebFileLocation = webFileLocation;
			Version = version;
			StackTrace = stackTrace;
			Userstory = userstory;
			DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			//Addseconds complains, 1 second = 10000 ns ticks so do that instead
			Timestamp = unixStart.AddTicks(unixTimeStamp * 10000);

			if (folderlocation != null)
			{
				Progress = CrashReportProcessingProgress.Unpacked;
			}
			else if (downloadedZip != null)
			{
				Progress = CrashReportProcessingProgress.Downloaded;
			}
			else
			{
				Progress = CrashReportProcessingProgress.NotStarted;
			}
		}

		private DownloaderWorker _worker;
		private CrashReportProcessingProgress _progress;

		internal void StartDownload(DownloaderWorker worker)
		{
			if (Progress != CrashReportProcessingProgress.NotStarted)
			{
				throw new InvalidOperationException();
			}

			Progress = CrashReportProcessingProgress.Downloading;

			string file = PersistentState.Database.GetKey("crashdumps_zip_folder") +  "\\" +  Guid + ".zip";
			_worker = worker;
			_worker.Enqueue(Guid, WebFileLocation, _key, file);

			_worker.StatusChanged += WorkerOnStatusChanged;
		}

		private void WorkerOnStatusChanged(INotifyThreadStatus sender, StatusChangedEventArgs args)
		{
			if (args.AttachedData?.guid != Guid ?? false)
				return;
			
			_worker.StatusChanged -= WorkerOnStatusChanged;
			
			_database.SetZipFileLocation(Guid, args.AttachedData.destinationPath);

			WebFileLocation = args.AttachedData.destinationPath;

			string userstory = null, exception;
			using (ZipArchive archive = new ZipArchive(File.OpenRead(args.AttachedData.destinationPath), ZipArchiveMode.Read, false))
			{
				ZipArchiveEntry userstoryEntry = archive.GetEntry("userstory.txt");
				if (userstoryEntry != null)
				{
					using (Stream s = userstoryEntry.Open())
					{
						byte[] buffer = new byte[userstoryEntry.Length];
						s.Read(buffer, 0, buffer.Length);
						userstory = Encoding.UTF8.GetString(buffer);
					}
				}

				ZipArchiveEntry exceptionEntry= archive.GetEntry("exception.txt");
				using (Stream s = exceptionEntry.Open())
				{
					byte[] buffer = new byte[exceptionEntry.Length];
					s.Read(buffer, 0, buffer.Length);
					exception = Encoding.UTF8.GetString(buffer);
				}
			}
			Userstory = userstory;
			StackTrace = exception;

			_database.SetStackTrace(Guid, exception);
			if (userstory != null) _database.SetUserStory(Guid, userstory);


			Progress = CrashReportProcessingProgress.Downloaded;
		}
	}

	public enum CrashReportProcessingProgress
	{
		NotStarted,
		Downloading,
		Downloaded,
		Unpacking,
		Unpacked
	}
}
