using System;

namespace ChummerDataViewer.Model
{
	class CrashReport
	{
		private readonly string _key;
		private readonly string _downloadedZip;
		private readonly string _folderlocation;

		public Guid Guid { get; }
		public string BuildType { get; set; }
		public string ErrorFrindly { get; set; }
		public string WebFileLocation { get; set; }
		public Version Version { get; set; }
		public DateTime Timestamp { get; }


		public CrashReport(Guid guid, long unixTimeStamp, string buildType, string errorFrindly, string key,
			string webFileLocation, Version version, string downloadedZip = null, string folderlocation = null)
		{
			_key = key;
			_downloadedZip = downloadedZip;
			_folderlocation = folderlocation;
			Guid = guid;
			BuildType = buildType;
			ErrorFrindly = errorFrindly;
			WebFileLocation = webFileLocation;
			Version = version;
			Timestamp = DateTime.FromFileTimeUtc(unixTimeStamp);

		}
	}
}
