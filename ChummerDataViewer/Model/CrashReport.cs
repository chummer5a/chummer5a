using System;

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
		public string BuildType { get; set; }
		public string ErrorFrindly { get; set; }
		public string WebFileLocation { get; set; }
		public Version Version { get; set; }
		public DateTime Timestamp { get; }

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
			string webFileLocation, Version version, string downloadedZip = null, string folderlocation = null)
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
