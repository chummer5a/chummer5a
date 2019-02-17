using System;
using System.IO;
using Amazon.Runtime;

namespace ChummerDataViewer.Model
{
	static class PersistentState
	{
		public static Database Database { get; private set; }

		public static string BulkFolder => _path.Value;
		public static string DatabaseFolder => _dbFolder.Value;


		public static bool Setup => Database != null;
		public static AWSCredentials AWSCredentials => Setup ? _awsCredentials.Value : null;

		public static void Initialize(string id, string key, string bulkStoragePath)
		{
			if (Database != null) return;

			Directory.CreateDirectory(DatabaseFolder);

			Database = Database.Create();
			Database.SetKey("crashdumps_aws_id", id);
			Database.SetKey("crashdumps_aws_key", key);
			Database.SetKey("crashdumps_zip_folder", bulkStoragePath);


			Directory.CreateDirectory(BulkFolder);
		}
		
		static PersistentState()
		{
			if (File.Exists(Path.Combine(DatabaseFolder, "persistent.db")))
			{
				Database = new Database();
			}
		}

		private static readonly Lazy<string> _path = new Lazy<string>(() => Database.GetKey("crashdumps_zip_folder"));

		private static readonly Lazy<string> _dbFolder =
			new Lazy<string>(
				() =>
					Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
						"ChummerCrashDump"));

		private static readonly Lazy<AWSCredentials> _awsCredentials =
			new Lazy<AWSCredentials>(() => new BasicAWSCredentials(Database.GetKey("crashdumps_aws_id"), Database.GetKey("crashdumps_aws_key")));
	}
}
