using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace ChummerDataViewer.Model
{
	internal class Database
	{
		private readonly object _syncRoot = new object();
		private DatabasePrivateApi innerApi;

		private readonly SQLiteConnection _dbConnection;
		private readonly SQLiteCommand _setKey;
		private readonly SQLiteCommand _deleteKey;
		private readonly SQLiteCommand _getKey;

		//Read crash
		private readonly SQLiteCommand _getAllCrashes;
		private readonly SQLiteCommand _getSingleCrash;
		private readonly SQLiteCommand _getDistinctVersions;
		private readonly SQLiteCommand _getDistinctBuildTypes;

		//write crash
		private readonly SQLiteCommand _insertCrash;
		private readonly SQLiteCommand _setZipFile;
		private readonly SQLiteCommand _setUserStory;
		private readonly SQLiteCommand _setStackTrace;

		private static void ExecuteSQL(SQLiteConnection connection, string sql)
		{
			using (SQLiteCommand cmd = new SQLiteCommand(sql, connection))
			{
				cmd.ExecuteNonQuery();
			}
		}

		public static Database Create()
		{
			SQLiteConnection connection =
				new SQLiteConnection($"Data Source={Path.Combine(PersistentState.DatabaseFolder, "persistent.db")}; Version=3;");
			connection.Open();
			ExecuteSQL(connection, "CREATE TABLE kvstore (key TEXT UNIQUE PRIMARY KEY, value TEXT);");

			ExecuteSQL(connection, "CREATE TABLE crashreports " +
			                       "(" +
			                       "guid TEXT UNIQUE PRIMARY KEY, " +
			                       "timestamp INTEGER, " +
			                       "buildtype TEXT, " +
			                       "errorfriendly TEXT, " +
			                       "encryptionkey TEXT, " +
			                       "url TEXT, " +
			                       "version TEXT, " +
			                       "ziplocation TEXT, " +
			                       "folderlocation TEXT, " +
			                       "userstory TEXT," +
			                       "stacktrace TEXT" +
			                       ");");

			ExecuteSQL(connection, "CREATE INDEX errortypeindex ON crashreports (errorfriendly);");
			ExecuteSQL(connection, "CREATE INDEX timeindex ON crashreports (timestamp);");
			ExecuteSQL(connection, "CREATE INDEX versionindex ON crashreports (version);");

			return new Database(connection);
		}

		private Database(SQLiteConnection conn)
		{
			_dbConnection = conn;
			_setKey = new SQLiteCommand("INSERT OR REPLACE INTO kvstore (key, value) VALUES (@key, @value);", _dbConnection);
			_deleteKey = new SQLiteCommand("DELETE FROM kvstore WHERE key=@key", _dbConnection);
			_getKey = new SQLiteCommand("SELECT value FROM kvstore WHERE key = @key;", _dbConnection);
			_insertCrash = new SQLiteCommand("INSERT OR IGNORE INTO crashreports " +
			                                 "(guid, timestamp, buildtype, errorfriendly, encryptionkey, url, version) " +
			                                 "VALUES " +
			                                 "(@guid, @timestamp, @buildtype, @errorfriendly, @encryptionkey, @url, @version);", 
				_dbConnection);

			_getAllCrashes = new SQLiteCommand("SELECT * FROM crashreports ORDER BY timestamp DESC;", _dbConnection);
			_getSingleCrash = new SQLiteCommand("SELECT * FROM crashreports WHERE guid=@guid", _dbConnection);
			_setZipFile = new SQLiteCommand("UPDATE crashreports SET ziplocation = @ziplocation WHERE guid = @guid", _dbConnection);
			_setUserStory = new SQLiteCommand("UPDATE crashreports SET userstory = @userstory WHERE guid = @guid", _dbConnection);
			_setStackTrace = new SQLiteCommand("UPDATE crashreports SET stacktrace = @stacktrace WHERE guid = @guid", _dbConnection);


			_getDistinctVersions = new SQLiteCommand("SELECT DISTINCT version FROM crashreports", _dbConnection);
			_getDistinctBuildTypes = new SQLiteCommand("SELECT DISTINCT buildtype FROM crashreports", _dbConnection);

			innerApi = new DatabasePrivateApi(this);
		}

		public Database() : this(
			new SQLiteConnection($"Data Source={Path.Combine(PersistentState.DatabaseFolder, "persistent.db")}; Version=3;")
				.OpenAndReturn())
		{
			
		}

		public void SetKey(string key, string value)
		{
			if(key == null) throw new ArgumentNullException(nameof(key));

			lock (_syncRoot)
			{
				if (value == null)
				{
					_deleteKey.Reset(true, false);
					_deleteKey.Parameters.Add(new SQLiteParameter("@key", key));

					_deleteKey.ExecuteNonQuery();
				}
				else
				{

					_setKey.Reset(true, false);
					_setKey.Parameters.Add(new SQLiteParameter("@key", key));
					_setKey.Parameters.Add(new SQLiteParameter("@value", value));

					_setKey.ExecuteNonQuery();
				}
			}
		}

		public string GetKey(string key)
		{
			lock (_syncRoot)
			{
				_getKey.Reset(true, false);
				_getKey.Parameters.Add(new SQLiteParameter("@key", key));

				using (SQLiteDataReader reader = _getKey.ExecuteReader())
				{
					while (reader.Read())
					{
						return reader["value"].ToString();
					}
				}

				return null;
			}
		}

		public SQLiteTransaction GetTransaction()
		{
			lock (_syncRoot)
			{
				return _dbConnection.BeginTransaction();
			}
		}

		public void CreateCrashReport(Guid guid, long timestamp, string buildType, string errorFriendly, string key, string webFileLocation, Version version)
		{
			lock (_syncRoot)
			{
				_insertCrash.Reset(true, false);
				_insertCrash.Parameters.Add(new SQLiteParameter("@guid", guid.ToString()));
				_insertCrash.Parameters.Add(new SQLiteParameter("@timestamp", timestamp));
				_insertCrash.Parameters.Add(new SQLiteParameter("@buildtype", buildType));
				_insertCrash.Parameters.Add(new SQLiteParameter("@errorfriendly", errorFriendly));
				_insertCrash.Parameters.Add(new SQLiteParameter("@encryptionkey", key));
				_insertCrash.Parameters.Add(new SQLiteParameter("@url", webFileLocation));
				_insertCrash.Parameters.Add(new SQLiteParameter("@version", version.ToString()));

				_insertCrash.ExecuteNonQuery();
			}
		}

		public IEnumerable<CrashReport> GetAllCrashes()
		{
			lock (_syncRoot)
			{
				_getAllCrashes.Reset(true, false);

				using (SQLiteDataReader reader = _getAllCrashes.ExecuteReader())
				{
					while (reader.Read())
					{
						yield return MakeCrashReport(reader);
					}
				}
			}
		}

		public IEnumerable<Version> GetAllVersions()
		{
			lock (_syncRoot)
			{
				_getDistinctVersions.Reset(true, false);

				using (SQLiteDataReader reader = _getDistinctVersions.ExecuteReader())
				{
					while (reader.Read())
					{
						yield return Version.Parse(reader.GetString(0));
					}
				}
			}
		}

		public IEnumerable<string> GetAllBuildTypes()
		{
			lock (_syncRoot)
			{
				_getDistinctBuildTypes.Reset(true, false);

				using (SQLiteDataReader reader = _getDistinctBuildTypes.ExecuteReader())
				{
					while (reader.Read())
					{
						yield return reader.GetString(0);
					}
				}
			}
		}

		private CrashReport MakeCrashReport(SQLiteDataReader reader)
		{
			Guid g = reader.GetGuid(0); //Guid.Parse(reader.GetString(0)));
			long l = reader.GetInt64(1);
			string s1 = reader.GetString(2);
			string s2 = reader.GetString(3);
			string s3 = reader.GetString(4);
			string s4 = reader.GetString(5);
			Version v = Version.Parse(reader.GetString(6));
			string s6 = reader.GetValue(7) as string;         //Not GetString as GetString throws an invalidCast 
			string s7 = reader.GetValue(8) as string;         //(or another exception, cannot rmbr)
			string userstory = reader.GetValue(9) as string;  //If the value is null, and this and following fields 
			string trace = reader.GetValue(10) as string;     //can be null as they aren't set before zip is downloaded


			return new CrashReport(innerApi, g, l, s1, s2, s3, s4, v, s6, s7, trace, userstory);
		}

		public CrashReport GetCrash(Guid guid)
		{
			lock (_syncRoot)
			{
				_getSingleCrash.Reset(true, false);
				_getSingleCrash.Parameters.Add(new SQLiteParameter("@guid", guid.ToString()));

				using (SQLiteDataReader reader = _getSingleCrash.ExecuteReader())
				{
					if (reader.HasRows && reader.Read())
					{
						return MakeCrashReport(reader);
					}
				}

				return null;
			}
		}

		private void SetZipFileLocation(Guid guid, string filePath)
		{
			lock (_syncRoot)
			{
				_setZipFile.Reset(true, false);
				_setZipFile.Parameters.Add(new SQLiteParameter("@guid", guid.ToString()));
				_setZipFile.Parameters.Add(new SQLiteParameter("@ziplocation", filePath));

				_setZipFile.ExecuteNonQuery();
			}
		}

		private void SetUserStory(Guid guid, string userstory)
		{
			lock (_syncRoot)
			{
				_setUserStory.Reset(true, false);
				_setUserStory.Parameters.Add(new SQLiteParameter("@guid", guid.ToString()));
				_setUserStory.Parameters.Add(new SQLiteParameter("@userstory", userstory));

				_setUserStory.ExecuteNonQuery();
			}
		}

		private void SetStackTrace(Guid guid, string exception)
		{
			lock (_syncRoot)
			{
				_setStackTrace.Reset(true, false);
				_setStackTrace.Parameters.Add(new SQLiteParameter("@guid", guid.ToString()));
				_setStackTrace.Parameters.Add(new SQLiteParameter("@stacktrace", exception));

				_setStackTrace.ExecuteNonQuery();
			}
		}

		public class DatabasePrivateApi
		{
			private Database _db;

			internal DatabasePrivateApi(Database db)
			{
				_db = db;
			}

			internal void SetZipFileLocation(Guid guid, string filePath) => _db.SetZipFileLocation(guid, filePath);

			public void SetStackTrace(Guid guid, string exception) => _db.SetStackTrace(guid, exception);

			public void SetUserStory(Guid guid, string userstory) => _db.SetUserStory(guid, userstory);
		}

		

		public void Delete()
		{
			lock (_syncRoot)
			{
				_dbConnection.Close();
				File.Delete(Path.Combine(PersistentState.DatabaseFolder, "persistent.db"));
			}
		}
	}
}