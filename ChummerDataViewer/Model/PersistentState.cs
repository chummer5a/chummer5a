using System;
using System.IO;
using Amazon.Runtime;

namespace ChummerDataViewer.Model
{
	static class PersistentState
	{
		public static Database Database { get; private set; }

		public static string FolderPath => _path.Value;

		public static bool Setup => Database != null;
		public static AWSCredentials AWSCredentials => Setup ? _awsCredentials.Value : null;

		public static void Initialize(string id, string key)
		{
			if (Database != null) return;

			Directory.CreateDirectory(FolderPath);

			Database = Database.Create();
			Database.SetKey("crashdumps_aws_id", id);
			Database.SetKey("crashdumps_aws_key", key);
		}
		
		static PersistentState()
		{
			if (File.Exists(Path.Combine(FolderPath, "persistent.db")))
			{
				Database = new Database();
			}
		}

		private static readonly Lazy<string> _path =
			new Lazy<string>(
				() =>
					Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
						"ChummerCrashDump"));

		private static readonly Lazy<AWSCredentials> _awsCredentials =
			new Lazy<AWSCredentials>(() => new BasicAWSCredentials(Database.GetKey("crashdumps_aws_id"), Database.GetKey("crashdumps_aws_key")));
	}
}
