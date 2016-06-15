using System;

namespace ChummerDataViewer.Model
{
	public class CrashReport
	{
		private readonly Database _database;
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


		internal CrashReport(Database database, Guid guid, long unixTimeStamp, string buildType, string errorFrindly, string key,
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
			
		}
	}
}
